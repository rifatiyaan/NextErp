# Contributing to NextERP backend

This document captures conventions enforced in `NextErp.*` projects. Read this before adding handlers, validators, or commands.

## Architecture at a glance

- **Clean Architecture** layers: `Domain` → `Application` → `Infrastructure` → `API`
- **CQRS via MediatR** — every write is a `Command`, every read is a `Query`
- **EF Core 8 with SQL Server** (Postgres migration in progress)
- **Single `ApplicationDbContext`** — no multi-DbContext, no separate UnitOfWork

## Data access — use `IApplicationDbContext` directly

The `IApplicationDbContext` interface (in `NextErp.Application/Interfaces`) exposes all `DbSet<T>` properties + `SaveChangesAsync`. **Inject this in handlers, not anything else.**

```csharp
public class CreateFooHandler(IApplicationDbContext db, IBranchProvider branchProvider)
    : IRequestHandler<CreateFooCommand, Guid>
{
    public async Task<Guid> Handle(CreateFooCommand request, CancellationToken ct = default)
    {
        var foo = new Foo { ... };
        db.Foos.Add(foo);
        await db.SaveChangesAsync(ct);
        return foo.Id;
    }
}
```

### Do NOT use

- ❌ `IApplicationUnitOfWork` — **removed** as of 2026-04
- ❌ `I*Repository` typed interfaces — **removed**
- ❌ Direct `DbContext` (concrete type) — use the interface

### Why this design

EF Core's `DbContext` is itself a Unit-of-Work + Repository implementation. Wrapping it in another layer adds zero value when there is exactly one DbContext, no DB-per-tenant, and no multi-context transactional needs.

## Transactional commands — mark them

Every command that performs **writes** must implement `ITransactionalRequest` (in `NextErp.Application.Common.Interfaces`). The `TransactionBehavior` MediatR pipeline wraps these in a DB transaction; an exception inside the handler triggers rollback.

```csharp
[RequiresPermission("Foo.Create")]
public record CreateFooCommand(string Name)
    : IRequest<Guid>, ITransactionalRequest;   // ← marker required
```

### Single-row writes also marked

Even single-`Add` commands implement `ITransactionalRequest` for consistency. There is no measurable performance penalty (EF wraps a single statement in an implicit transaction anyway), and it future-proofs the command if a second write is added later.

### Queries do NOT mark

Read-only commands (`Get*`, `List*`) implement only `IRequest<T>`, not `ITransactionalRequest`.

## Validation — FluentValidation

Place validators in `NextErp.Application/Validators/<Area>/<CommandName>Validator.cs`. Validators are auto-discovered from the assembly at startup.

```csharp
public sealed class CreateFooCommandValidator : AbstractValidator<CreateFooCommand>
{
    public CreateFooCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
```

### Rule of thumb — validation vs business rules

| Goes in validator | Goes in handler |
|---|---|
| Required, length, range, regex, enum | Entity exists in DB |
| Static reference list membership | Branch context required |
| Field-shape rules | Negative-stock prevention |
| Cross-field shape (e.g. EndDate ≥ StartDate) | Anything that needs DB or service state |

Validators run before handlers in the pipeline. A failed validator throws `ValidationException` which the API maps to **HTTP 422** with a `errors` dictionary keyed by field name.

## Conditional LINQ — use `WhereIf` / `IncludeIf` extensions

For queries with optional filters, prefer the conditional LINQ extensions in
`NextErp.Application/Common/Extensions/QueryableExtensions.cs` over fragmented `if` statements.

### Available extensions (IQueryable)

| Method | Applies when | Common use |
|---|---|---|
| `WhereIf(condition, predicate)` | `bool == true` | Toggle filter, e.g. `IsActive` flag |
| `WhereIfHasValue(value, predicate)` | `Nullable<T>.HasValue` | Optional ID filters (`request.PartyId`) |
| `WhereIfNotEmpty(string?, predicate)` | `!IsNullOrWhiteSpace` | Search text (whitespace = empty) |
| `WhereIfNotNullOrEmpty(string?, predicate)` | `!IsNullOrEmpty` | Search text (whitespace = present) |
| `WhereIfAny(collection, predicate)` | non-null + Count > 0 | "in" filters |
| `IncludeIf(condition, path)` | `bool == true` | Optional eager loads |
| `OrderByIf(condition, key)` | `bool == true` | Optional sort |
| `OrderByDescendingIf(condition, key)` | `bool == true` | Optional desc sort |
| `PageIf(paged, pageIndex, pageSize)` | `bool == true` | Optional paging |

### Before / after

```csharp
// ❌ Before — fragmented, hard to read
var query = dbContext.Sales.AsNoTracking();
if (!string.IsNullOrWhiteSpace(request.SearchText))
    query = query.Where(s => s.Title.Contains(request.SearchText));
if (request.PartyId.HasValue)
    query = query.Where(s => s.PartyId == request.PartyId.Value);

// ✅ After — fluent chain, top-to-bottom readable
var query = dbContext.Sales
    .AsNoTracking()
    .WhereIfNotEmpty(request.SearchText, s => s.Title.Contains(request.SearchText!))
    .WhereIfHasValue(request.PartyId, s => s.PartyId == request.PartyId!.Value);
```

### Same SQL — pure readability sugar

The generated SQL is **identical** to the imperative form. No runtime overhead, no extra round-trips.

### When NOT to use

- **Many-way switches** — keep `switch` expressions for sort by string column (`"title" => OrderBy(...)`, etc.)
- **Composite predicates with `OR` short-circuiting** that read clearly as a single `Where` — don't break apart artificially
- **`string.IsNullOrEmpty` semantic** (whitespace = present) — extension uses `IsNullOrWhiteSpace`. Add a separate overload if you need the other semantic.

### `IEnumerable` variants

`EnumerableExtensions.WhereIf*` mirrors the IQueryable shape but takes `Func<T, bool>` for in-memory predicates.

## Multi-item write handlers — batch-load to avoid N+1

Handlers that operate on multiple items (sale, purchase, bulk transfer) must **load all variants and stocks once** at the top, then perform mutations purely in-memory. Anything else turns into N+1 query hell.

### Pattern

```csharp
public async Task<Guid> Handle(CreateXxxCommand request, CancellationToken ct = default)
{
    // Phase 1: variants in 1 query
    var variantIds = request.Items.Select(i => i.ProductVariantId).Distinct().ToList();
    var variants = await stockService.LoadVariantsAsync(variantIds, ct);

    // ... resolve branch / tenant ...

    // Phase 2: stocks for branch in 1 query
    var ctx = await stockService.LoadStockContextAsync(variants, branchId, tenantId, ct);

    // Phase 3: validate + mutate purely in-memory (no DB calls)
    foreach (var line in request.Items)
    {
        if (!stockService.HasStockAvailable(ctx, line.ProductVariantId, line.Quantity))
            throw new InvalidOperationException(...);

        stockService.RecordMovement(ctx, line.ProductVariantId, -line.Quantity,
            StockMovementType.Sale, sale.Id);
    }

    // Phase 4: single round-trip persists everything
    await dbContext.SaveChangesAsync(ct);
}
```

### Do NOT do this

```csharp
// ❌ BAD — re-queries variant + stock per loop iteration (N+1)
foreach (var item in request.Items)
{
    await stockService.RecordMovementAsync(item.ProductVariantId, ...);
}
```

### Why it matters

For a 5-item sale, the naive per-item path runs **~30 queries**. The batch path runs **3** (variants + stocks + SaveChanges).

### When to use which API

| Handler shape | API |
|---|---|
| Single-item write (e.g. `CreateStockAdjustmentCommand`) | Existing `RecordMovementAsync` (per-item async) |
| Multi-item write (e.g. `CreateSale`, `CreatePurchase`) | `LoadVariantsAsync` + `LoadStockContextAsync` + sync `RecordMovement` |

## Permissions — `[RequiresPermission]`

Annotate command/query records with the permission key:

```csharp
[RequiresPermission("Foo.Create")]
public record CreateFooCommand(...) : IRequest<Guid>, ITransactionalRequest;
```

The `PermissionBehavior` pipeline checks the user's permissions and throws `ForbiddenAccessException` (mapped to HTTP 403) when missing. Super-admin bypasses the check.

## Cancellation tokens

Every async method takes `CancellationToken cancellationToken = default` and propagates it to all downstream `*Async` calls. Default value means call sites do not need to thread one through unless they have a real token.

```csharp
public async Task<Foo?> GetAsync(int id, CancellationToken cancellationToken = default)
    => await db.Foos.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
```

## Testing — xUnit + SQLite + NSubstitute

Tests live in `NextErp.Application.Tests`. Three concerns covered today:

| Folder | What goes there |
|---|---|
| `Validators/` | FluentValidation rules — pure logic, no DB |
| `Handlers/<Area>/` | Handler tests via `HandlerTestBase` (SQLite in-memory) |
| `Behaviors/` | Pipeline behavior tests (Validation, Permission, Transaction) |

### Pattern for a handler test

```csharp
public class CreateFooHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Happy_path_inserts_row()
    {
        var sut = new CreateFooHandler(Db, BranchProvider);
        var cmd = new CreateFooCommand("Test");

        var id = await sut.Handle(cmd, CancellationToken.None);

        var persisted = await Db.Foos.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
        persisted.Should().NotBeNull();
    }
}
```

### Builders

Add a builder to `Builders/` for any entity you need to seed in tests. Follow the fluent style of existing builders (`ProductVariantBuilder`, `BranchBuilder`).

### What NOT to mock

- ❌ `DbContext` — use SQLite via `HandlerTestBase`
- ❌ `AutoMapper` profiles — let real mapping run
- ❌ `MediatR` — instantiate handlers directly

### What to mock

- ✅ `IBranchProvider`, `IUserContext` — already wired in `HandlerTestBase`
- ✅ `ILogger<T>` — use `NullLogger<T>.Instance`
- ✅ External services where interaction needs verifying

## Running tests

```bash
# Kill API if running (Windows DLL locks):
taskkill //F //IM NextErp.API.exe

# Single project:
dotnet test NextErp.Application.Tests/NextErp.Application.Tests.csproj

# Full solution:
dotnet test NextErp.sln
```

CI runs `dotnet test` on every push to `main`. Failing tests block deploy.

## Conventions checklist for new commands

When adding a new write command:

- [ ] Record placed in `NextErp.Application/Commands/<Area>/`
- [ ] Implements `IRequest<TResponse>` AND `ITransactionalRequest`
- [ ] Has `[RequiresPermission("Foo.Action")]` attribute
- [ ] Validator added in `Validators/<Area>/` if shape rules apply
- [ ] Handler injects `IApplicationDbContext` (not UoW, not repository)
- [ ] Handler tests added in `Handlers/<Area>/`
- [ ] Permission key registered in seeding (if new permission)

## Norwegian customer / compliance notes

- ISO 8601 dates, 24-hour time, metric units in any user-facing output
- StockMovement is append-only — never `Update` or `Remove` a movement row (audit trail)
- Soft-delete entities (`ISoftDeletable`) → set `IsActive = false`, never `db.Foos.Remove(...)`
- GDPR: do not log raw user data in Serilog — use property names but redact values where sensitive

## Planned architectural refactor — "May 2026 architecture"

The owner has decided to eventually refactor NextErp from the current
**direct-`IApplicationDbContext`** style to the **strict Repository + UnitOfWork
pattern** used by the sibling Certain QMS Checklist codebase
(`C:\netpower\netpower.qms.backend.checklist`).

### Trigger phrase

When the user (or any future Claude/Codex agent operating on their behalf) says
**"refactor may 2026 architecture"** — execute the refactor described below.
No further clarification needed; the scope is fixed in this document.

### Scope of the refactor

1. **Base abstractions** in `NextErp.Application/Common/Persistence/`
   - `IRepository<TEntity>` — `GetByIdAsync`, `ExistsAsync`, `AddAsync`, `Update`, `Remove`.
     **DO NOT** expose `IQueryable<T>` or a `Query()` method — that defeats the
     pattern's purpose (see "leaky repository" critique).
   - `IUnitOfWork` — `SaveChangesAsync`, `BeginTransactionAsync`.
   - `ITransactionScope` — `CommitAsync`, `RollbackAsync`, `DisposeAsync`.

2. **Generic implementation** in `NextErp.Infrastructure/Persistence/`
   - `Repository<TEntity>` — base impl wrapping `DbContext.Set<TEntity>()`.
   - `UnitOfWork` — delegates to `DbContext.SaveChangesAsync` / `BeginTransactionAsync`.

3. **Per-entity repository interfaces + implementations** for every entity in
   `NextErp.Domain/Entities/` — ~23 entities. Each interface lives in
   `NextErp.Application/<Area>/Abstractions/` and the impl in
   `NextErp.Infrastructure/<Area>/Repositories/`. Each interface adds the
   specific named query methods that handlers need
   (`GetByIdWithDetailsAsync`, `GetPagedAsync`, etc.) — no `IQueryable<T>` ever
   leaves the impl.

4. **Migrate ~50 handlers** in `NextErp.Application/Handlers/` from
   `IApplicationDbContext` injection to `IUnitOfWork` + the appropriate
   `I<Entity>Repository` injections. Replace inline LINQ with named repo
   method calls.

5. **Architecture tests** (new test project
   `NextErp.ArchitectureTests/`) enforcing:
   - No `Microsoft.EntityFrameworkCore` namespace reference inside
     `NextErp.Application.*` (excluding `Persistence` abstractions).
   - All handlers must inject `IRepository<T>` interfaces, never the
     `DbContext` or `IApplicationDbContext`.
   - All write commands must implement `ITransactionalRequest`.

6. **Update existing test infrastructure** (`HandlerTestBase`) to support
   either real-DB repositories (preferred, mirrors current SQLite in-memory
   strategy) or Moq-style mock repositories (per-test choice).

7. **Remove or repurpose `IApplicationDbContext`** — keep it only as the
   single shared abstraction injected into `Repository<TEntity>` and
   `UnitOfWork`, never elsewhere.

8. **Update this file**: flip the "Do NOT use" anti-pattern notes for
   `IApplicationUnitOfWork` / `I*Repository` — those become the *required*
   patterns post-refactor.

### Estimated effort

16-24 hours focused work (~2-3 full days). Phaseable:

- R-1 (~3h): base abstractions + Product entity end-to-end
- R-2 (~5h): remaining simple-CRUD entities (Customer, Supplier, Category, Branch)
- R-3 (~6h): aggregate entities (Sale, Purchase, Stock with their child entities)
- R-4 (~3h): Accounting + Loyalty entities
- R-5 (~2h): Architecture tests + docs + final cleanup

### Rationale (recorded for the future agent)

The current "direct-DbContext" style is correct for NextErp's solo-pet
context — the QMS pattern's costs (boilerplate, named methods per query)
don't pay off without arch tests, Moq-heavy tests, multi-dev teams, or
regulated compliance. The owner is choosing to refactor anyway for:

- **Learning** — hands-on practice of the strict pattern used at work
- **Portfolio** — demonstrable enterprise-grade architecture
- **Mental consistency** — same pattern across all owner's codebases

So when executing the refactor: do NOT propose half-measures (Hybrid /
`Query()` returning `IQueryable`). The whole point is the discipline of
the strict pattern. Half-measures defeat the learning + portfolio goals.
