# Ecommerce Backend (Plan 1 of 3) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Backend for the public storefront: publication flags, ecommerce settings, anonymous `api/store/*` catalog + order intake, and the OnlineOrder → confirm → Sale pipeline.

**Architecture:** Follows the spec `docs/superpowers/specs/2026-07-03-ecommerce-storefront-design.md`. New `OnlineOrder` aggregate holds price snapshots; staff confirmation creates the Sale through the existing `CreateSaleCommand` pipeline. Public store queries bypass the branch global filter with `IgnoreQueryFilters()` + an explicit `BranchId == EcommerceSettings.SellingBranchId` predicate.

**Tech Stack:** .NET 8, EF Core 8 (SQL Server; SQLite in tests via `HandlerTestBase`), MediatR CQRS, FluentValidation (auto-discovered pipeline), Mapperly-style static mappers, xUnit + FluentAssertions + NSubstitute.

## Global Constraints

- TDD for every handler/factory: failing test first, watch it fail, minimal code, watch it pass, commit.
- Enums cross the wire as strings (global `JsonStringEnumConverter`) — frontend uses string unions.
- No XML doc summaries that restate structure; comments only for non-obvious WHY.
- Public DTOs must never expose `Cost`, `TenantId`, `BranchId`, or admin-only fields.
- UTC timestamps; `CreatedAt`/`UpdatedAt` conventions as elsewhere.
- Test run command: `dotnet test NextErp.Application.Tests/NextErp.Application.Tests.csproj --nologo`
- All git commands run from `C:\Personal\nexterp\NextErp`. Commit messages end with `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>`.
- The API process must not be running during builds (DLL copy-locks).

---

### Task 1: Publication flags on Product and Category + migration

**Files:**
- Modify: `NextErp.Domain/Entities/Product.cs` (after line 31 `IsActive`)
- Modify: `NextErp.Domain/Entities/Category.cs` (after line 20 `IsActive`)
- Test: `NextErp.Application.Tests/Handlers/Ecommerce/PublicationFlagTests.cs` (new folder `Handlers/Ecommerce`)

**Interfaces:**
- Produces: `Product.IsPublishedOnline : bool` and `Category.IsPublishedOnline : bool`, both default `false`. Every later task filters on these.

- [ ] **Step 1: Write the failing test**

```csharp
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class PublicationFlagTests : HandlerTestBase
{
    [Fact]
    public async Task Products_and_categories_default_to_unpublished()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(500).WithTitle("Cat").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithCode("P900001").WithCategory(500).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();

        (await Db.Products.AsNoTracking().FirstAsync()).IsPublishedOnline.Should().BeFalse();
        (await Db.Categories.AsNoTracking().FirstAsync()).IsPublishedOnline.Should().BeFalse();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test NextErp.Application.Tests/NextErp.Application.Tests.csproj --filter "FullyQualifiedName~PublicationFlagTests" --nologo`
Expected: FAIL — compile error `'Product' does not contain a definition for 'IsPublishedOnline'`.

- [ ] **Step 3: Add the properties**

In `Product.cs` directly under `public bool IsActive { get; set; } = true;`:

```csharp
        // Storefront curation: only products explicitly published (and whose
        // category is published) appear on the public store.
        public bool IsPublishedOnline { get; set; } = false;
```

In `Category.cs` directly under `public bool IsActive { get; set; } = true;`:

```csharp
        public bool IsPublishedOnline { get; set; } = false;
```

- [ ] **Step 4: Run test to verify it passes**

Run: same command as Step 2. Expected: PASS (SQLite test context rebuilds the model from the entities; no migration needed for tests).

- [ ] **Step 5: Add the SQL Server migration**

Run from `C:\Personal\nexterp\NextErp`:
```powershell
dotnet ef migrations add AddOnlinePublicationFlags --project NextErp.Infrastructure --startup-project NextErp.API
dotnet build NextErp.sln -nologo
```
Expected: migration file created under `NextErp.Infrastructure/Migrations/`, build 0 errors. Do NOT run `database update` here — the repo's `run-migrations.ps1` handles that at deploy/dev time.

- [ ] **Step 6: Commit**

```powershell
git add -A; git commit -m "feat(ecommerce): add IsPublishedOnline flags to Product and Category

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 2: EcommerceSettings settings module

**Files:**
- Create: `NextErp.Application/Settings/EcommerceSettings.cs`
- Test: `NextErp.Application.Tests/Services/SettingsProviderTests.cs` (append one test; existing file, mirror its arrange style)

**Interfaces:**
- Consumes: `ISettingsProvider.GetAsync<T>()` / `UpdateAsync(T)` (`NextErp.Application.Common.Settings`).
- Produces: `EcommerceSettings` with properties `StoreName`, `Tagline`, `HeroHeadline`, `HeroImageUrl`, `MarqueeText`, `CodNote`, `DeliveryFee`, `SellingBranchId`, `StorefrontEnabled`. Auto-appears in the existing settings schema/values API (`GetFeatureSettingsSchemaHandler` discovers `[SettingsModule]` classes by assembly scan).

- [ ] **Step 1: Write the failing test** (append inside the existing `SettingsProviderTests` class)

```csharp
    [Fact]
    public async Task Ecommerce_settings_round_trip_defaults_and_update()
    {
        var sut = BuildProvider(); // reuse the class's existing factory helper; if named differently, use the same construction the sibling tests use

        var defaults = await sut.GetAsync<EcommerceSettings>();
        defaults.StorefrontEnabled.Should().BeFalse();
        defaults.DeliveryFee.Should().Be(0m);
        defaults.StoreName.Should().Be("NextErp Store");

        defaults.StorefrontEnabled = true;
        defaults.DeliveryFee = 60m;
        await sut.UpdateAsync(defaults);

        var roundTrip = await sut.GetAsync<EcommerceSettings>();
        roundTrip.StorefrontEnabled.Should().BeTrue();
        roundTrip.DeliveryFee.Should().Be(60m);
    }
```
(Note: open `SettingsProviderTests.cs` first and copy the exact provider construction used at line ~56; the helper name above is illustrative of location, not of API.)

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test NextErp.Application.Tests/NextErp.Application.Tests.csproj --filter "FullyQualifiedName~Ecommerce_settings_round_trip" --nologo`
Expected: FAIL — `EcommerceSettings` not found.

- [ ] **Step 3: Create the settings class** (mirrors `SalesSettings.cs`)

```csharp
using NextErp.Application.Common.Settings;

namespace NextErp.Application.Settings;

[SettingsModule("Ecommerce", "Ecommerce / Storefront")]
public sealed class EcommerceSettings
{
    [Setting(
        description: "Master switch. Off = every public store endpoint returns 403 and the storefront shows a closed page.",
        displayName: "Storefront enabled")]
    public bool StorefrontEnabled { get; set; } = false;

    [Setting(description: "Public store name shown in the header and page titles.", displayName: "Store name")]
    public string StoreName { get; set; } = "NextErp Store";

    [Setting(description: "Short tagline under the store name (optional).", displayName: "Tagline")]
    public string Tagline { get; set; } = "";

    [Setting(description: "Homepage hero headline.", displayName: "Hero headline")]
    public string HeroHeadline { get; set; } = "Objects, honestly made.";

    [Setting(description: "Homepage hero image URL (optional).", displayName: "Hero image URL")]
    public string HeroImageUrl { get; set; } = "";

    [Setting(description: "Marquee ribbon text on the homepage.", displayName: "Marquee text")]
    public string MarqueeText { get; set; } = "Cash on delivery — no account needed";

    [Setting(description: "Short cash-on-delivery explanation shown at checkout.", displayName: "COD note")]
    public string CodNote { get; set; } = "Pay in cash when your order arrives.";

    [Setting(description: "Flat delivery fee added to every online order.", displayName: "Delivery fee")]
    [SettingRange(0, 100000)]
    public decimal DeliveryFee { get; set; } = 0m;

    [Setting(description: "Branch whose stock and orders the storefront uses.", displayName: "Selling branch id")]
    public string SellingBranchId { get; set; } = "";
}
```
`SellingBranchId` is a string (Guid text) because the `[Setting]` system stores JSON primitives; parse with `Guid.TryParse` at read sites.

- [ ] **Step 4: Run test to verify it passes**

Run: same filter. Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add -A; git commit -m "feat(ecommerce): add Ecommerce settings module

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 3: OnlineOrder + OnlineOrderItem entities, EF config, DbSets, migration

**Files:**
- Create: `NextErp.Domain/Entities/OnlineOrder.cs`
- Create: `NextErp.Infrastructure/Configurations/OnlineOrderConfiguration.cs`
- Modify: `NextErp.Application/Interfaces/IApplicationDbContext.cs` (add two DbSets near `DbSet<Sale>`)
- Modify: `NextErp.Infrastructure/ApplicationDbContext.cs` (add the same two DbSets; apply configuration if the context applies configs individually — check how `CategoryConfiguration` is registered and mirror it)
- Test: `NextErp.Application.Tests/Handlers/Ecommerce/OnlineOrderEntityTests.cs`

**Interfaces:**
- Produces:
```csharp
public enum OnlineOrderStatus { Pending = 0, Confirmed = 1, Cancelled = 2 }
public class OnlineOrder  // NOT [BranchScoped]: staff see all branches' online orders
{
    int Id; string OrderNumber; string CustomerName; string Phone; string Address; string? Note;
    OnlineOrderStatus Status; string? CancelReason; decimal DeliveryFee;
    Guid? PartyId; Guid? SaleId; Guid TenantId; Guid BranchId;
    DateTime CreatedAt; DateTime? ConfirmedAt;
    ICollection<OnlineOrderItem> Items;
}
public class OnlineOrderItem
{
    int Id; int OnlineOrderId; OnlineOrder OnlineOrder;
    int ProductVariantId; string ProductTitle; string Sku; decimal UnitPrice; decimal Quantity; decimal LineTotal;
}
```
- `IApplicationDbContext.OnlineOrders` and `.OnlineOrderItems`.

- [ ] **Step 1: Write the failing test**

```csharp
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class OnlineOrderEntityTests : HandlerTestBase
{
    [Fact]
    public async Task Online_order_with_items_persists_and_loads()
    {
        var order = new OnlineOrder
        {
            OrderNumber = "W000001",
            CustomerName = "Test Customer",
            Phone = "01700000000",
            Address = "12 Example Road, Dhaka",
            DeliveryFee = 60m,
            TenantId = TenantId,
            BranchId = BranchId,
            CreatedAt = DateTime.UtcNow,
            Items =
            {
                new OnlineOrderItem
                {
                    ProductVariantId = 1,
                    ProductTitle = "Sample",
                    Sku = "P000001a",
                    UnitPrice = 100m,
                    Quantity = 2m,
                    LineTotal = 200m,
                },
            },
        };
        Db.OnlineOrders.Add(order);
        await Db.SaveChangesAsync();

        var fresh = await Db.OnlineOrders.AsNoTracking()
            .Include(o => o.Items)
            .FirstAsync(o => o.OrderNumber == "W000001");
        fresh.Status.Should().Be(OnlineOrderStatus.Pending);
        fresh.Items.Should().ContainSingle(i => i.LineTotal == 200m);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test NextErp.Application.Tests/NextErp.Application.Tests.csproj --filter "FullyQualifiedName~OnlineOrderEntityTests" --nologo`
Expected: FAIL — `OnlineOrder` not found.

- [ ] **Step 3: Create the entity**

`NextErp.Domain/Entities/OnlineOrder.cs`:
```csharp
using NextErp.Domain.Common;

namespace NextErp.Domain.Entities
{
    public enum OnlineOrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2,
    }

    public class OnlineOrder : IEntity<int>
    {
        public int Id { get; set; }

        // W + 6-digit tenant-sequential number; the customer-facing reference.
        public string OrderNumber { get; set; } = null!;

        public string CustomerName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? Note { get; set; }

        public OnlineOrderStatus Status { get; set; } = OnlineOrderStatus.Pending;
        public string? CancelReason { get; set; }

        // Snapshot of the flat fee quoted at order time.
        public decimal DeliveryFee { get; set; }

        public Guid? PartyId { get; set; }
        public Party? Party { get; set; }
        public Guid? SaleId { get; set; }
        public Sale? Sale { get; set; }

        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        public ICollection<OnlineOrderItem> Items { get; set; } = new List<OnlineOrderItem>();
    }

    public class OnlineOrderItem : IEntity<int>
    {
        public int Id { get; set; }
        public int OnlineOrderId { get; set; }
        public OnlineOrder OnlineOrder { get; set; } = null!;

        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;

        // Snapshots — exactly what the customer saw and agreed to.
        public string ProductTitle { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
```

- [ ] **Step 4: EF configuration**

`NextErp.Infrastructure/Configurations/OnlineOrderConfiguration.cs` (mirror the builder style of `CategoryConfiguration.cs`):
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class OnlineOrderConfiguration : IEntityTypeConfiguration<OnlineOrder>
{
    public void Configure(EntityTypeBuilder<OnlineOrder> builder)
    {
        builder.Property(o => o.OrderNumber).HasMaxLength(16).IsRequired();
        builder.HasIndex(o => new { o.TenantId, o.OrderNumber }).IsUnique();
        builder.Property(o => o.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Phone).HasMaxLength(32).IsRequired();
        builder.Property(o => o.Address).HasMaxLength(1000).IsRequired();
        builder.Property(o => o.Note).HasMaxLength(1000);
        builder.Property(o => o.CancelReason).HasMaxLength(500);
        builder.Property(o => o.DeliveryFee).HasPrecision(18, 2);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.OnlineOrder)
            .HasForeignKey(i => i.OnlineOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Party).WithMany().HasForeignKey(o => o.PartyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Sale).WithMany().HasForeignKey(o => o.SaleId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class OnlineOrderItemConfiguration : IEntityTypeConfiguration<OnlineOrderItem>
{
    public void Configure(EntityTypeBuilder<OnlineOrderItem> builder)
    {
        builder.Property(i => i.ProductTitle).HasMaxLength(300).IsRequired();
        builder.Property(i => i.Sku).HasMaxLength(64).IsRequired();
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.Quantity).HasPrecision(18, 3);
        builder.Property(i => i.LineTotal).HasPrecision(18, 2);

        builder.HasOne(i => i.ProductVariant).WithMany().HasForeignKey(i => i.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
    }
}
```
If `ApplicationDbContext.OnModelCreating` uses `ApplyConfigurationsFromAssembly`, nothing more is needed; if it applies configurations one by one, add both there.

- [ ] **Step 5: Add DbSets**

In `IApplicationDbContext.cs` after `DbSet<SalePayment> SalePayments { get; set; }`:
```csharp
        DbSet<OnlineOrder> OnlineOrders { get; set; }
        DbSet<OnlineOrderItem> OnlineOrderItems { get; set; }
```
Add the identical two properties to `ApplicationDbContext`.

- [ ] **Step 6: Run test to verify it passes**

Run: same filter as Step 2. Expected: PASS.

- [ ] **Step 7: Migration + build + commit**

```powershell
dotnet ef migrations add AddOnlineOrders --project NextErp.Infrastructure --startup-project NextErp.API
dotnet build NextErp.sln -nologo
git add -A; git commit -m "feat(ecommerce): add OnlineOrder aggregate with item snapshots

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 4: OnlineOrderNumberFactory (W + 6-digit tenant sequence)

**Files:**
- Create: `NextErp.Application/Ecommerce/OnlineOrderNumberFactory.cs`
- Test: `NextErp.Application.Tests/Handlers/Ecommerce/OnlineOrderNumberFactoryTests.cs`

**Interfaces:**
- Produces: `static Task<string> OnlineOrderNumberFactory.NextNumberAsync(Guid tenantId, IApplicationDbContext dbContext, CancellationToken ct = default)` → `"W000001"`, continues from tenant max. Mirrors `Products/ProductCodeFactory.cs`.

- [ ] **Step 1: Write the failing test**

```csharp
using NextErp.Application.Ecommerce;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class OnlineOrderNumberFactoryTests : HandlerTestBase
{
    private OnlineOrder OrderWithNumber(string number) => new()
    {
        OrderNumber = number,
        CustomerName = "C",
        Phone = "017",
        Address = "A",
        TenantId = TenantId,
        BranchId = BranchId,
        CreatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task First_number_is_W000001()
    {
        var number = await OnlineOrderNumberFactory.NextNumberAsync(TenantId, Db);
        number.Should().Be("W000001");
    }

    [Fact]
    public async Task Continues_from_existing_max()
    {
        Db.OnlineOrders.Add(OrderWithNumber("W000007"));
        await Db.SaveChangesAsync();

        var number = await OnlineOrderNumberFactory.NextNumberAsync(TenantId, Db);
        number.Should().Be("W000008");
    }
}
```

- [ ] **Step 2: Run to verify FAIL** (`--filter "FullyQualifiedName~OnlineOrderNumberFactoryTests"`) — type not found.

- [ ] **Step 3: Implement** (copy the `ProductCodeFactory` scan pattern)

```csharp
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Ecommerce;

public static class OnlineOrderNumberFactory
{
    private const string Prefix = "W";
    private const int Digits = 6;

    public static async Task<string> NextNumberAsync(
        Guid tenantId,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.OnlineOrders
            .AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.OrderNumber.StartsWith(Prefix))
            .Select(o => o.OrderNumber)
            .ToListAsync(cancellationToken);

        var max = 0;
        foreach (var number in existing)
        {
            if (number.Length == Prefix.Length + Digits
                && int.TryParse(number.Substring(Prefix.Length), out var n)
                && n > max)
            {
                max = n;
            }
        }

        return Prefix + (max + 1).ToString("D" + Digits);
    }
}
```

- [ ] **Step 4: Run to verify PASS.**
- [ ] **Step 5: Commit** (`feat(ecommerce): sequential online order numbers`, same footer).

---

### Task 5: Publication admin — read tree + bulk update

**Files:**
- Create: `NextErp.Application/Queries/Ecommerce/EcommerceQueries.cs`
- Create: `NextErp.Application/Commands/Ecommerce/EcommerceCommands.cs`
- Create: `NextErp.Application/DTOs/Ecommerce/Responses/PublicationResponses.cs`
- Create: `NextErp.Application/Handlers/QueryHandlers/Ecommerce/GetEcommercePublicationHandler.cs`
- Create: `NextErp.Application/Handlers/CommandHandlers/Ecommerce/SetEcommercePublicationHandler.cs`
- Test: `NextErp.Application.Tests/Handlers/Ecommerce/PublicationHandlersTests.cs`

**Interfaces:**
- Produces:
```csharp
// Queries
public record GetEcommercePublicationQuery() : IRequest<List<PublicationCategoryResponse>>;
// Commands ([RequiresPermission("Settings.System.Manage")], ITransactionalRequest)
public record SetEcommercePublicationCommand(
    List<int> PublishCategoryIds, List<int> UnpublishCategoryIds,
    List<int> PublishProductIds, List<int> UnpublishProductIds) : IRequest<Unit>;
// DTOs
public sealed record PublicationProductRow(int Id, string Title, string Code, decimal Price, bool IsPublishedOnline);
public sealed record PublicationCategoryResponse(int Id, string Title, int? ParentId, bool IsPublishedOnline, List<PublicationProductRow> Products);
```

- [ ] **Step 1: Write the failing tests**

```csharp
using MediatR;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Handlers.CommandHandlers.Ecommerce;
using NextErp.Application.Handlers.QueryHandlers.Ecommerce;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class PublicationHandlersTests : HandlerTestBase
{
    private const int CatA = 600;
    private const int CatB = 601;

    private async Task SeedAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(CatA).WithTitle("A").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(CatB).WithTitle("B").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder().WithId(6000).WithTitle("P1").WithCode("P600001").WithCategory(CatA).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder().WithId(6001).WithTitle("P2").WithCode("P600002").WithCategory(CatB).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Tree_returns_categories_with_products_and_flags()
    {
        await SeedAsync();
        var sut = new GetEcommercePublicationHandler(Db);

        var tree = await sut.Handle(new GetEcommercePublicationQuery(), CancellationToken.None);

        tree.Should().HaveCount(2);
        tree.Single(c => c.Id == CatA).Products.Should().ContainSingle(p => p.Code == "P600001" && !p.IsPublishedOnline);
    }

    [Fact]
    public async Task Bulk_update_sets_and_clears_flags()
    {
        await SeedAsync();
        var sut = new SetEcommercePublicationHandler(Db);

        await sut.Handle(new SetEcommercePublicationCommand(
            PublishCategoryIds: new() { CatA },
            UnpublishCategoryIds: new(),
            PublishProductIds: new() { 6000 },
            UnpublishProductIds: new()), CancellationToken.None);

        (await Db.Categories.AsNoTracking().FirstAsync(c => c.Id == CatA)).IsPublishedOnline.Should().BeTrue();
        (await Db.Products.AsNoTracking().FirstAsync(p => p.Id == 6000)).IsPublishedOnline.Should().BeTrue();
        (await Db.Products.AsNoTracking().FirstAsync(p => p.Id == 6001)).IsPublishedOnline.Should().BeFalse();

        await sut.Handle(new SetEcommercePublicationCommand(
            new(), new() { CatA }, new(), new() { 6000 }), CancellationToken.None);
        (await Db.Categories.AsNoTracking().FirstAsync(c => c.Id == CatA)).IsPublishedOnline.Should().BeFalse();
        (await Db.Products.AsNoTracking().FirstAsync(p => p.Id == 6000)).IsPublishedOnline.Should().BeFalse();
    }
}
```

- [ ] **Step 2: Run to verify FAIL** (types missing).

- [ ] **Step 3: Implement**

`Queries/Ecommerce/EcommerceQueries.cs`:
```csharp
using MediatR;
using NextErp.Application.DTOs.Ecommerce;

namespace NextErp.Application.Queries.Ecommerce
{
    public record GetEcommercePublicationQuery() : IRequest<List<PublicationCategoryResponse>>;
}
```

`Commands/Ecommerce/EcommerceCommands.cs`:
```csharp
using MediatR;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Interfaces;

namespace NextErp.Application.Commands.Ecommerce
{
    [RequiresPermission("Settings.System.Manage")]
    public record SetEcommercePublicationCommand(
        List<int> PublishCategoryIds,
        List<int> UnpublishCategoryIds,
        List<int> PublishProductIds,
        List<int> UnpublishProductIds
    ) : IRequest<Unit>, ITransactionalRequest;
}
```

`DTOs/Ecommerce/Responses/PublicationResponses.cs`:
```csharp
namespace NextErp.Application.DTOs.Ecommerce;

public sealed record PublicationProductRow(int Id, string Title, string Code, decimal Price, bool IsPublishedOnline);

public sealed record PublicationCategoryResponse(
    int Id, string Title, int? ParentId, bool IsPublishedOnline, List<PublicationProductRow> Products);
```

`Handlers/QueryHandlers/Ecommerce/GetEcommercePublicationHandler.cs`:
```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Ecommerce;

namespace NextErp.Application.Handlers.QueryHandlers.Ecommerce;

public class GetEcommercePublicationHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetEcommercePublicationQuery, List<PublicationCategoryResponse>>
{
    public async Task<List<PublicationCategoryResponse>> Handle(
        GetEcommercePublicationQuery request, CancellationToken cancellationToken = default)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Title)
            .Select(c => new { c.Id, c.Title, c.ParentId, c.IsPublishedOnline })
            .ToListAsync(cancellationToken);

        // Admin curation view intentionally spans branches: one batched product
        // query, grouped in memory (no N+1).
        var products = await dbContext.Products
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new { p.Id, p.Title, p.Code, p.Price, p.IsPublishedOnline, p.CategoryId })
            .ToListAsync(cancellationToken);
        var byCategory = products.GroupBy(p => p.CategoryId).ToDictionary(g => g.Key, g => g.ToList());

        return categories.Select(c => new PublicationCategoryResponse(
            c.Id, c.Title, c.ParentId, c.IsPublishedOnline,
            (byCategory.GetValueOrDefault(c.Id) ?? new())
                .Select(p => new PublicationProductRow(p.Id, p.Title, p.Code, p.Price, p.IsPublishedOnline))
                .ToList()))
            .ToList();
    }
}
```

`Handlers/CommandHandlers/Ecommerce/SetEcommercePublicationHandler.cs`:
```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.Ecommerce;

public class SetEcommercePublicationHandler(IApplicationDbContext dbContext)
    : IRequestHandler<SetEcommercePublicationCommand, Unit>
{
    public async Task<Unit> Handle(SetEcommercePublicationCommand request, CancellationToken cancellationToken = default)
    {
        var categoryIds = request.PublishCategoryIds.Concat(request.UnpublishCategoryIds).ToList();
        var categories = await dbContext.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);
        foreach (var category in categories)
            category.IsPublishedOnline = request.PublishCategoryIds.Contains(category.Id);

        var productIds = request.PublishProductIds.Concat(request.UnpublishProductIds).ToList();
        var products = await dbContext.Products
            .IgnoreQueryFilters()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
        foreach (var product in products)
            product.IsPublishedOnline = request.PublishProductIds.Contains(product.Id);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
```

- [ ] **Step 4: Run to verify PASS**, then run the FULL suite (`dotnet test ... --nologo`) — 0 failures.
- [ ] **Step 5: Commit** (`feat(ecommerce): publication tree query and bulk publish/unpublish`).

---

### Task 6: Public store queries (config, categories, products, product detail)

**Files:**
- Create: `NextErp.Application/DTOs/Ecommerce/Responses/StoreResponses.cs`
- Create: `NextErp.Application/Queries/Ecommerce/StoreQueries.cs`
- Create: `NextErp.Application/Handlers/QueryHandlers/Ecommerce/StoreQueryHandlers.cs`
- Test: `NextErp.Application.Tests/Handlers/Ecommerce/StoreQueryHandlersTests.cs`

**Interfaces:**
- Consumes: `EcommerceSettings` via `ISettingsProvider.GetAsync<EcommerceSettings>()`, publication flags, `Stocks` for availability.
- Produces:
```csharp
public record GetStoreConfigQuery() : IRequest<StoreConfigResponse>;
public record GetStoreCategoriesQuery() : IRequest<List<StoreCategoryResponse>>;
public record GetStorePagedProductsQuery(int? CategoryId, string? SearchText, int PageIndex = 1, int PageSize = 24) : IRequest<StorePagedProductsResponse>;
public record GetStoreProductByIdQuery(int Id) : IRequest<StoreProductDetailResponse?>;

public sealed record StoreConfigResponse(bool StorefrontEnabled, string StoreName, string Tagline, string HeroHeadline, string HeroImageUrl, string MarqueeText, string CodNote, decimal DeliveryFee);
public sealed record StoreCategoryResponse(int Id, string Title, int? ParentId, int ProductCount, string? ImageUrl);
public sealed record StoreProductRow(int Id, string Title, decimal Price, string? ImageUrl, string? SecondImageUrl, bool InStock, decimal? LowStockQuantity, bool HasVariations);
public sealed record StorePagedProductsResponse(int Total, List<StoreProductRow> Data);
public sealed record StoreVariantRow(int Id, string Sku, string Title, decimal Price, bool InStock, decimal? LowStockQuantity);
public sealed record StoreProductDetailResponse(int Id, string Title, decimal Price, string? Description, string? CategoryTitle, int CategoryId, List<string> Images, List<StoreVariantRow> Variants);
```
- Availability rule: `InStock = availableQuantity > 0`; `LowStockQuantity = quantity when 0 < quantity <= 5, else null`. Exact large counts are never exposed.
- Visibility rule (every product query): `p.IsActive && p.IsPublishedOnline && p.Category.IsPublishedOnline && p.Category.IsActive`, `IgnoreQueryFilters()` + `p.BranchId == sellingBranchId`.

- [ ] **Step 1: Write the failing tests**

```csharp
using NextErp.Application.Common.Settings;
using NextErp.Application.Handlers.QueryHandlers.Ecommerce;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Application.Settings;
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class StoreQueryHandlersTests : HandlerTestBase
{
    private const int PublishedCat = 700;
    private const int UnpublishedCat = 701;

    private ISettingsProvider SettingsWith(Guid sellingBranchId) =>
        SettingsProviderReturning(new EcommerceSettings
        {
            StorefrontEnabled = true,
            SellingBranchId = sellingBranchId.ToString(),
            DeliveryFee = 60m,
        });

    private static ISettingsProvider SettingsProviderReturning(EcommerceSettings settings)
    {
        var provider = Substitute.For<ISettingsProvider>();
        provider.GetAsync<EcommerceSettings>().Returns(Task.FromResult(settings));
        return provider;
    }

    private async Task SeedCatalogAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        var published = new CategoryBuilder().WithId(PublishedCat).WithTitle("Published").WithTenant(TenantId).WithBranch(BranchId).Build();
        published.IsPublishedOnline = true;
        Db.Categories.Add(published);
        Db.Categories.Add(new CategoryBuilder().WithId(UnpublishedCat).WithTitle("Hidden").WithTenant(TenantId).WithBranch(BranchId).Build());

        var visible = new ProductBuilder().WithId(7000).WithTitle("Visible").WithCode("P700001").WithPrice(100m)
            .WithCategory(PublishedCat).WithTenant(TenantId).WithBranch(BranchId).Build();
        visible.IsPublishedOnline = true;
        Db.Products.Add(visible);

        // Unpublished product in a published category — must not appear.
        Db.Products.Add(new ProductBuilder().WithId(7001).WithTitle("Unpublished").WithCode("P700002")
            .WithCategory(PublishedCat).WithTenant(TenantId).WithBranch(BranchId).Build());

        // Published product in an UNpublished category — must not appear.
        var orphan = new ProductBuilder().WithId(7002).WithTitle("OrphanPublished").WithCode("P700003")
            .WithCategory(UnpublishedCat).WithTenant(TenantId).WithBranch(BranchId).Build();
        orphan.IsPublishedOnline = true;
        Db.Products.Add(orphan);

        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Products_returns_only_published_products_in_published_categories()
    {
        await SeedCatalogAsync();
        var sut = new GetStorePagedProductsHandler(Db, SettingsWith(BranchId));

        var page = await sut.Handle(new GetStorePagedProductsQuery(null, null), CancellationToken.None);

        page.Total.Should().Be(1);
        page.Data.Should().ContainSingle(p => p.Title == "Visible");
    }

    [Fact]
    public async Task Products_in_another_branch_are_excluded()
    {
        await SeedCatalogAsync();
        var otherBranch = Guid.NewGuid();
        var sut = new GetStorePagedProductsHandler(Db, SettingsWith(otherBranch));

        var page = await sut.Handle(new GetStorePagedProductsQuery(null, null), CancellationToken.None);

        page.Total.Should().Be(0);
    }

    [Fact]
    public async Task Categories_returns_published_with_counts()
    {
        await SeedCatalogAsync();
        var sut = new GetStoreCategoriesHandler(Db, SettingsWith(BranchId));

        var categories = await sut.Handle(new GetStoreCategoriesQuery(), CancellationToken.None);

        categories.Should().ContainSingle(c => c.Title == "Published" && c.ProductCount == 1);
    }

    [Fact]
    public async Task Config_reflects_settings()
    {
        var sut = new GetStoreConfigHandler(SettingsWith(BranchId));

        var config = await sut.Handle(new GetStoreConfigQuery(), CancellationToken.None);

        config.StorefrontEnabled.Should().BeTrue();
        config.DeliveryFee.Should().Be(60m);
    }

    [Fact]
    public async Task Detail_returns_null_for_unpublished_product()
    {
        await SeedCatalogAsync();
        var sut = new GetStoreProductByIdHandler(Db, SettingsWith(BranchId));

        (await sut.Handle(new GetStoreProductByIdQuery(7001), CancellationToken.None)).Should().BeNull();
        (await sut.Handle(new GetStoreProductByIdQuery(7000), CancellationToken.None)).Should().NotBeNull();
    }
}
```

- [ ] **Step 2: Run to verify FAIL** (`--filter "FullyQualifiedName~StoreQueryHandlersTests"`).

- [ ] **Step 3: Implement**

`DTOs/Ecommerce/Responses/StoreResponses.cs`: the records exactly as in **Interfaces** above, namespace `NextErp.Application.DTOs.Ecommerce`.

`Queries/Ecommerce/StoreQueries.cs`:
```csharp
using MediatR;
using NextErp.Application.DTOs.Ecommerce;

namespace NextErp.Application.Queries.Ecommerce
{
    public record GetStoreConfigQuery() : IRequest<StoreConfigResponse>;
    public record GetStoreCategoriesQuery() : IRequest<List<StoreCategoryResponse>>;
    public record GetStorePagedProductsQuery(
        int? CategoryId, string? SearchText, int PageIndex = 1, int PageSize = 24)
        : IRequest<StorePagedProductsResponse>;
    public record GetStoreProductByIdQuery(int Id) : IRequest<StoreProductDetailResponse?>;
}
```

`Handlers/QueryHandlers/Ecommerce/StoreQueryHandlers.cs`:
```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Common.Settings;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Application.Settings;

namespace NextErp.Application.Handlers.QueryHandlers.Ecommerce;

internal static class StoreQueryShared
{
    // Anonymous requests carry no branch claim, so the [BranchScoped] global
    // filter cannot apply — every store query bypasses it and pins the branch
    // to the configured selling branch explicitly.
    public static async Task<Guid> SellingBranchAsync(ISettingsProvider settings)
    {
        var s = await settings.GetAsync<EcommerceSettings>();
        return Guid.TryParse(s.SellingBranchId, out var id) ? id : Guid.Empty;
    }

    public static decimal? LowStock(decimal available) =>
        available > 0 && available <= 5 ? available : null;
}

public class GetStoreConfigHandler(ISettingsProvider settings)
    : IRequestHandler<GetStoreConfigQuery, StoreConfigResponse>
{
    public async Task<StoreConfigResponse> Handle(GetStoreConfigQuery request, CancellationToken cancellationToken = default)
    {
        var s = await settings.GetAsync<EcommerceSettings>();
        return new StoreConfigResponse(
            s.StorefrontEnabled, s.StoreName, s.Tagline, s.HeroHeadline,
            s.HeroImageUrl, s.MarqueeText, s.CodNote, s.DeliveryFee);
    }
}

public class GetStoreCategoriesHandler(IApplicationDbContext dbContext, ISettingsProvider settings)
    : IRequestHandler<GetStoreCategoriesQuery, List<StoreCategoryResponse>>
{
    public async Task<List<StoreCategoryResponse>> Handle(GetStoreCategoriesQuery request, CancellationToken cancellationToken = default)
    {
        var branchId = await StoreQueryShared.SellingBranchAsync(settings);

        var categories = await dbContext.Categories
            .AsNoTracking()
            .Where(c => c.IsActive && c.IsPublishedOnline)
            .OrderBy(c => c.Title)
            .Select(c => new
            {
                c.Id, c.Title, c.ParentId,
                ImageUrl = c.Assets.Where(a => a.Type == "image").Select(a => a.Url).FirstOrDefault(),
                ProductCount = dbContext.Products
                    .IgnoreQueryFilters()
                    .Count(p => p.CategoryId == c.Id && p.IsActive && p.IsPublishedOnline && p.BranchId == branchId),
            })
            .ToListAsync(cancellationToken);

        return categories
            .Where(c => c.ProductCount > 0)
            .Select(c => new StoreCategoryResponse(c.Id, c.Title, c.ParentId, c.ProductCount, c.ImageUrl))
            .ToList();
    }
}

public class GetStorePagedProductsHandler(IApplicationDbContext dbContext, ISettingsProvider settings)
    : IRequestHandler<GetStorePagedProductsQuery, StorePagedProductsResponse>
{
    public async Task<StorePagedProductsResponse> Handle(GetStorePagedProductsQuery request, CancellationToken cancellationToken = default)
    {
        var branchId = await StoreQueryShared.SellingBranchAsync(settings);
        var pageIndex = Math.Max(1, request.PageIndex);
        var pageSize = Math.Clamp(request.PageSize, 1, 60);

        var query = dbContext.Products
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.IsActive && p.IsPublishedOnline && p.BranchId == branchId
                        && p.Category.IsActive && p.Category.IsPublishedOnline);

        if (request.CategoryId is int categoryId)
            query = query.Where(p => p.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(request.SearchText))
            query = query.Where(p => p.Title.Contains(request.SearchText));

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(p => p.Title)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id, p.Title, p.Price, p.HasVariations,
                Images = p.ProductImages.OrderBy(i => i.DisplayOrder).Select(i => i.Url).Take(2).ToList(),
                FallbackImage = p.ImageUrl,
                Available = dbContext.Stocks
                    .IgnoreQueryFilters()
                    .Where(s => s.BranchId == branchId
                                && p.ProductVariants.Select(v => v.Id).Contains(s.ProductVariantId))
                    .Sum(s => (decimal?)s.AvailableQuantity) ?? 0m,
            })
            .ToListAsync(cancellationToken);

        var data = rows.Select(r => new StoreProductRow(
            r.Id, r.Title, r.Price,
            r.Images.ElementAtOrDefault(0) ?? r.FallbackImage,
            r.Images.ElementAtOrDefault(1),
            r.Available > 0,
            StoreQueryShared.LowStock(r.Available),
            r.HasVariations)).ToList();

        return new StorePagedProductsResponse(total, data);
    }
}

public class GetStoreProductByIdHandler(IApplicationDbContext dbContext, ISettingsProvider settings)
    : IRequestHandler<GetStoreProductByIdQuery, StoreProductDetailResponse?>
{
    public async Task<StoreProductDetailResponse?> Handle(GetStoreProductByIdQuery request, CancellationToken cancellationToken = default)
    {
        var branchId = await StoreQueryShared.SellingBranchAsync(settings);

        var product = await dbContext.Products
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.Id == request.Id
                        && p.IsActive && p.IsPublishedOnline && p.BranchId == branchId
                        && p.Category.IsActive && p.Category.IsPublishedOnline)
            .Select(p => new
            {
                p.Id, p.Title, p.Price,
                Description = p.Metadata.Description,
                CategoryTitle = p.Category.Title,
                p.CategoryId,
                Images = p.ProductImages.OrderBy(i => i.DisplayOrder).Select(i => i.Url).ToList(),
                FallbackImage = p.ImageUrl,
                Variants = p.ProductVariants
                    .Where(v => v.IsActive)
                    .Select(v => new { v.Id, v.Sku, v.Title, v.Price })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            return null;

        var variantIds = product.Variants.Select(v => v.Id).ToList();
        var stockByVariant = await dbContext.Stocks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && variantIds.Contains(s.ProductVariantId))
            .GroupBy(s => s.ProductVariantId)
            .Select(g => new { VariantId = g.Key, Available = g.Sum(s => s.AvailableQuantity) })
            .ToDictionaryAsync(x => x.VariantId, x => x.Available, cancellationToken);

        var images = product.Images.Count > 0
            ? product.Images
            : (product.FallbackImage is null ? new List<string>() : new List<string> { product.FallbackImage });

        var variants = product.Variants.Select(v =>
        {
            var available = stockByVariant.GetValueOrDefault(v.Id, 0m);
            return new StoreVariantRow(v.Id, v.Sku, v.Title, v.Price, available > 0, StoreQueryShared.LowStock(available));
        }).ToList();

        return new StoreProductDetailResponse(
            product.Id, product.Title, product.Price, product.Description,
            product.CategoryTitle, product.CategoryId, images, variants);
    }
}
```
Note for the implementer: `ProductVariant` has `IsActive`, `Sku`, `Title`, `Price` (see `Handlers/Product` tests). `Stock` rows are per variant+branch with `AvailableQuantity` (see `StockService`). If the SQLite provider rejects the nested `Available` subquery in the list handler, materialize variant ids per page first and aggregate exactly like the detail handler does — keep it one extra query for the whole page, never per row.

- [ ] **Step 4: Run to verify PASS**, then full suite — 0 failures.
- [ ] **Step 5: Commit** (`feat(ecommerce): public store catalog queries`).

---

### Task 7: CreateOnlineOrderCommand + validator + handler

**Files:**
- Create: `NextErp.Application/DTOs/Ecommerce/Requests/StoreOrderRequests.cs`
- Modify: `NextErp.Application/Commands/Ecommerce/EcommerceCommands.cs` (append)
- Create: `NextErp.Application/Validators/Ecommerce/CreateOnlineOrderCommandValidator.cs`
- Create: `NextErp.Application/Handlers/CommandHandlers/Ecommerce/CreateOnlineOrderHandler.cs`
- Test: `NextErp.Application.Tests/Handlers/Ecommerce/CreateOnlineOrderHandlerTests.cs`

**Interfaces:**
- Consumes: `OnlineOrderNumberFactory.NextNumberAsync`, `ISettingsProvider.GetAsync<EcommerceSettings>()`, `INotificationService.RecordAsync(type, title, message, relatedEntityType, relatedEntityId, cancellationToken)`.
- Produces:
```csharp
public sealed record StoreOrderItemRequest(int ProductVariantId, decimal Quantity);
// ITransactionalRequest; NO RequiresPermission (anonymous)
public record CreateOnlineOrderCommand(
    string CustomerName, string Phone, string Address, string? Note,
    List<StoreOrderItemRequest> Items) : IRequest<string>; // returns OrderNumber
```
- Server computes all prices from the DB (client prices are never trusted). Snapshot = variant price at order time. `DeliveryFee` snapshot from settings. Validation failures throw the standard `ValidationException` handled by the existing pipeline.

- [ ] **Step 1: Write the failing tests**

```csharp
using FluentValidation;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Common.Settings;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Handlers.CommandHandlers.Ecommerce;
using NextErp.Application.Settings;
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class CreateOnlineOrderHandlerTests : HandlerTestBase
{
    private const int Cat = 800;
    private int _variantId;

    private ISettingsProvider Settings()
    {
        var provider = Substitute.For<ISettingsProvider>();
        provider.GetAsync<EcommerceSettings>().Returns(Task.FromResult(new EcommerceSettings
        {
            StorefrontEnabled = true,
            SellingBranchId = BranchId.ToString(),
            DeliveryFee = 60m,
        }));
        return provider;
    }

    private CreateOnlineOrderHandler BuildHandler() => new(Db, Settings(), Notifications);

    private async Task SeedPublishedProductAsync(bool published = true)
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        var cat = new CategoryBuilder().WithId(Cat).WithTitle("Cat").WithTenant(TenantId).WithBranch(BranchId).Build();
        cat.IsPublishedOnline = true;
        Db.Categories.Add(cat);
        var product = new ProductBuilder().WithId(8000).WithTitle("Widget").WithCode("P800001").WithPrice(150m)
            .WithCategory(Cat).WithTenant(TenantId).WithBranch(BranchId).Build();
        product.IsPublishedOnline = published;
        Db.Products.Add(product);
        var variant = SimpleProductVariantFactory.CreateDefault(product);
        variant.TenantId = TenantId;
        variant.BranchId = BranchId;
        Db.ProductVariants.Add(variant);
        await Db.SaveChangesAsync();
        _variantId = variant.Id;
    }

    [Fact]
    public async Task Creates_pending_order_with_snapshots_and_number()
    {
        await SeedPublishedProductAsync();
        var sut = BuildHandler();

        var orderNumber = await sut.Handle(new CreateOnlineOrderCommand(
            "Test Customer", "01700000000", "12 Example Road", null,
            new() { new StoreOrderItemRequest(_variantId, 2m) }), CancellationToken.None);

        orderNumber.Should().Be("W000001");
        var order = await Db.OnlineOrders.AsNoTracking().Include(o => o.Items).FirstAsync();
        order.Status.Should().Be(OnlineOrderStatus.Pending);
        order.DeliveryFee.Should().Be(60m);
        order.BranchId.Should().Be(BranchId);
        order.Items.Should().ContainSingle();
        order.Items.First().UnitPrice.Should().Be(150m);   // server-side snapshot, not client input
        order.Items.First().LineTotal.Should().Be(300m);
        order.Items.First().Sku.Should().Be("P800001-DEFAULT");
    }

    [Fact]
    public async Task Rejects_unpublished_product()
    {
        await SeedPublishedProductAsync(published: false);
        var sut = BuildHandler();

        var act = async () => await sut.Handle(new CreateOnlineOrderCommand(
            "Test Customer", "01700000000", "12 Example Road", null,
            new() { new StoreOrderItemRequest(_variantId, 1m) }), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Records_notification_for_staff()
    {
        await SeedPublishedProductAsync();
        var sut = BuildHandler();

        await sut.Handle(new CreateOnlineOrderCommand(
            "Test Customer", "01700000000", "12 Example Road", null,
            new() { new StoreOrderItemRequest(_variantId, 1m) }), CancellationToken.None);

        (await Db.Notifications.AsNoTracking().ToListAsync())
            .Should().ContainSingle(n => n.Type == "OnlineOrderPlaced");
    }
}
```

- [ ] **Step 2: Run to verify FAIL.**

- [ ] **Step 3: Implement**

`DTOs/Ecommerce/Requests/StoreOrderRequests.cs`:
```csharp
namespace NextErp.Application.DTOs.Ecommerce;

public sealed record StoreOrderItemRequest(int ProductVariantId, decimal Quantity);

public sealed class StoreOrderCreateRequest
{
    public string CustomerName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Note { get; set; }
    public List<StoreOrderItemRequest> Items { get; set; } = new();

    // Honeypot: humans never see it, bots fill it. Checked in the controller.
    public string? Website { get; set; }
}
```

Append to `Commands/Ecommerce/EcommerceCommands.cs`:
```csharp
    public record CreateOnlineOrderCommand(
        string CustomerName,
        string Phone,
        string Address,
        string? Note,
        List<NextErp.Application.DTOs.Ecommerce.StoreOrderItemRequest> Items
    ) : IRequest<string>, ITransactionalRequest;
```

`Validators/Ecommerce/CreateOnlineOrderCommandValidator.cs` (input-shape rules; catalog rules live in the handler where the DB is available):
```csharp
using FluentValidation;
using NextErp.Application.Commands.Ecommerce;

namespace NextErp.Application.Validators.Ecommerce;

public class CreateOnlineOrderCommandValidator : AbstractValidator<CreateOnlineOrderCommand>
{
    public CreateOnlineOrderCommandValidator()
    {
        RuleFor(c => c.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Phone).NotEmpty().MaximumLength(32)
            .Matches(@"^[0-9+\-\s()]{6,}$").WithMessage("Phone number looks invalid.");
        RuleFor(c => c.Address).NotEmpty().MaximumLength(1000);
        RuleFor(c => c.Note).MaximumLength(1000);
        RuleFor(c => c.Items).NotEmpty().WithMessage("The cart is empty.");
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductVariantId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).InclusiveBetween(1, 99);
        });
    }
}
```

`Handlers/CommandHandlers/Ecommerce/CreateOnlineOrderHandler.cs`:
```csharp
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Common.Settings;
using NextErp.Application.Ecommerce;
using NextErp.Application.Interfaces;
using NextErp.Application.Settings;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Ecommerce;

public class CreateOnlineOrderHandler(
    IApplicationDbContext dbContext,
    ISettingsProvider settingsProvider,
    INotificationService notifications)
    : IRequestHandler<CreateOnlineOrderCommand, string>
{
    public async Task<string> Handle(CreateOnlineOrderCommand request, CancellationToken cancellationToken = default)
    {
        var settings = await settingsProvider.GetAsync<EcommerceSettings>();
        if (!settings.StorefrontEnabled || !Guid.TryParse(settings.SellingBranchId, out var branchId))
            throw new InvalidOperationException("The storefront is not available.");

        var variantIds = request.Items.Select(i => i.ProductVariantId).Distinct().ToList();

        // One batched load; loop over the in-memory map (no N+1). Prices are
        // resolved server-side — the client never supplies a price.
        var variants = await dbContext.ProductVariants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(v => variantIds.Contains(v.Id)
                        && v.IsActive
                        && v.Product.IsActive && v.Product.IsPublishedOnline
                        && v.Product.BranchId == branchId
                        && v.Product.Category.IsActive && v.Product.Category.IsPublishedOnline)
            .Select(v => new { v.Id, v.Sku, v.Price, ProductTitle = v.Product.Title, TenantId = v.Product.TenantId })
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        var failures = request.Items
            .Where(i => !variants.ContainsKey(i.ProductVariantId))
            .Select(i => new ValidationFailure("Items", $"Item {i.ProductVariantId} is not available in the store."))
            .ToList();
        if (failures.Count > 0)
            throw new ValidationException(failures);

        var tenantId = variants.Values.First().TenantId;
        var order = new Entities.OnlineOrder
        {
            OrderNumber = await OnlineOrderNumberFactory.NextNumberAsync(tenantId, dbContext, cancellationToken),
            CustomerName = request.CustomerName.Trim(),
            Phone = request.Phone.Trim(),
            Address = request.Address.Trim(),
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            DeliveryFee = settings.DeliveryFee,
            TenantId = tenantId,
            BranchId = branchId,
            CreatedAt = DateTime.UtcNow,
        };
        foreach (var item in request.Items)
        {
            var variant = variants[item.ProductVariantId];
            order.Items.Add(new Entities.OnlineOrderItem
            {
                ProductVariantId = variant.Id,
                ProductTitle = variant.ProductTitle,
                Sku = variant.Sku,
                UnitPrice = variant.Price,
                Quantity = item.Quantity,
                LineTotal = decimal.Round(variant.Price * item.Quantity, 2),
            });
        }

        dbContext.OnlineOrders.Add(order);

        await notifications.RecordAsync(
            type: "OnlineOrderPlaced",
            title: "New online order",
            message: $"{order.OrderNumber} — {order.CustomerName}",
            relatedEntityType: "OnlineOrder",
            relatedEntityId: order.OrderNumber,
            cancellationToken: cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return order.OrderNumber;
    }
}
```

- [ ] **Step 4: Run to verify PASS**, then full suite.
- [ ] **Step 5: Commit** (`feat(ecommerce): guest online order intake with price snapshots`).

---

### Task 8: Confirm and Cancel handlers

**Files:**
- Modify: `NextErp.Application/Commands/Ecommerce/EcommerceCommands.cs` (append)
- Create: `NextErp.Application/Handlers/CommandHandlers/Ecommerce/ConfirmOnlineOrderHandler.cs`
- Create: `NextErp.Application/Handlers/CommandHandlers/Ecommerce/CancelOnlineOrderHandler.cs`
- Test: `NextErp.Application.Tests/Handlers/Ecommerce/ConfirmOnlineOrderHandlerTests.cs`

**Interfaces:**
- Consumes: `CreateSaleCommand(Guid? PartyId, decimal Discount, string? PaymentMethod, decimal? PaidAmount, List<SaleDto.SaleItemRequest> Items)` sent via `IMediator` (its handler moves stock and applies nothing extra when per-line prices are given: `SaleItemRequest { ProductVariantId, Quantity, Price, Subtotal }`).
- Produces:
```csharp
[RequiresPermission("Sale.Create")]
public record ConfirmOnlineOrderCommand(int Id) : IRequest<Guid>; // returns SaleId, ITransactionalRequest
[RequiresPermission("Sale.Create")]
public record CancelOnlineOrderCommand(int Id, string Reason) : IRequest<Unit>; // ITransactionalRequest
```
- Confirm: order must be `Pending`; order.BranchId must equal `IBranchProvider.GetRequiredBranchId()` (staff must operate in the selling branch — Sale pipeline stamps the current branch); Party matched by exact `Phone` (active, `PartyType.Customer`) else created; Sale created with snapshot prices (promotion engine NOT re-run — quoted totals are honored); on success set `PartyId`, `SaleId`, `Status=Confirmed`, `ConfirmedAt`. If the Sale pipeline throws (e.g. insufficient stock), the exception propagates and — because `ITransactionalRequest` wraps the whole confirm — the order stays untouched Pending.
- Known v1 limitation (documented in spec): `DeliveryFee` is tracked on the OnlineOrder only; the Sale contains product lines only.

- [ ] **Step 1: Write the failing tests**

```csharp
using MediatR;
using NextErp.Application.Commands;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Handlers.CommandHandlers.Ecommerce;
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class ConfirmOnlineOrderHandlerTests : HandlerTestBase
{
    private OnlineOrder SeedOrder(string phone = "01700000000")
    {
        var order = new OnlineOrder
        {
            OrderNumber = "W000001",
            CustomerName = "Test Customer",
            Phone = phone,
            Address = "12 Example Road",
            DeliveryFee = 60m,
            TenantId = TenantId,
            BranchId = BranchId,
            CreatedAt = DateTime.UtcNow,
            Items =
            {
                new OnlineOrderItem
                {
                    ProductVariantId = 42, ProductTitle = "Widget", Sku = "P800001a",
                    UnitPrice = 150m, Quantity = 2m, LineTotal = 300m,
                },
            },
        };
        Db.OnlineOrders.Add(order);
        Db.SaveChanges();
        return order;
    }

    private (ConfirmOnlineOrderHandler sut, IMediator mediator) BuildHandler(Guid? saleId = null)
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSaleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(saleId ?? Guid.NewGuid()));
        return (new ConfirmOnlineOrderHandler(Db, mediator, BranchProvider), mediator);
    }

    [Fact]
    public async Task Confirm_creates_party_sends_sale_with_snapshot_prices_and_links()
    {
        var order = SeedOrder();
        var expectedSaleId = Guid.NewGuid();
        var (sut, mediator) = BuildHandler(expectedSaleId);

        var saleId = await sut.Handle(new ConfirmOnlineOrderCommand(order.Id), CancellationToken.None);

        saleId.Should().Be(expectedSaleId);
        await mediator.Received(1).Send(
            Arg.Is<CreateSaleCommand>(c =>
                c.Items.Count == 1 &&
                c.Items[0].ProductVariantId == 42 &&
                c.Items[0].Price == 150m &&
                c.Items[0].Quantity == 2m &&
                c.Discount == 0m),
            Arg.Any<CancellationToken>());

        var fresh = await Db.OnlineOrders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
        fresh.Status.Should().Be(OnlineOrderStatus.Confirmed);
        fresh.SaleId.Should().Be(expectedSaleId);
        fresh.PartyId.Should().NotBeNull();

        var party = await Db.Parties.AsNoTracking().FirstAsync(p => p.Id == fresh.PartyId);
        party.Phone.Should().Be("01700000000");
        party.PartyType.Should().Be(PartyType.Customer);
    }

    [Fact]
    public async Task Confirm_reuses_existing_party_matched_by_phone()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        var existing = new Party
        {
            Id = Guid.NewGuid(), Title = "Existing", Phone = "01700000000",
            PartyType = PartyType.Customer, TenantId = TenantId, BranchId = BranchId,
            CreatedAt = DateTime.UtcNow,
        };
        Db.Parties.Add(existing);
        await Db.SaveChangesAsync();
        var order = SeedOrder();
        var (sut, _) = BuildHandler();

        await sut.Handle(new ConfirmOnlineOrderCommand(order.Id), CancellationToken.None);

        (await Db.OnlineOrders.AsNoTracking().FirstAsync(o => o.Id == order.Id))
            .PartyId.Should().Be(existing.Id);
        (await Db.Parties.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Confirm_rejects_non_pending_order()
    {
        var order = SeedOrder();
        order.Status = OnlineOrderStatus.Cancelled;
        Db.SaveChanges();
        var (sut, _) = BuildHandler();

        var act = async () => await sut.Handle(new ConfirmOnlineOrderCommand(order.Id), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Pending*");
    }

    [Fact]
    public async Task Cancel_requires_reason_and_sets_status()
    {
        var order = SeedOrder();
        var sut = new CancelOnlineOrderHandler(Db);

        await sut.Handle(new CancelOnlineOrderCommand(order.Id, "Customer unreachable"), CancellationToken.None);

        var fresh = await Db.OnlineOrders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
        fresh.Status.Should().Be(OnlineOrderStatus.Cancelled);
        fresh.CancelReason.Should().Be("Customer unreachable");
    }
}
```

- [ ] **Step 2: Run to verify FAIL.**

- [ ] **Step 3: Implement**

Append to `Commands/Ecommerce/EcommerceCommands.cs`:
```csharp
    [RequiresPermission("Sale.Create")]
    public record ConfirmOnlineOrderCommand(int Id) : IRequest<Guid>, ITransactionalRequest;

    [RequiresPermission("Sale.Create")]
    public record CancelOnlineOrderCommand(int Id, string Reason) : IRequest<Unit>, ITransactionalRequest;
```

`Handlers/CommandHandlers/Ecommerce/ConfirmOnlineOrderHandler.cs`:
```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;
using SaleDto = NextErp.Application.DTOs.Sale;

namespace NextErp.Application.Handlers.CommandHandlers.Ecommerce;

public class ConfirmOnlineOrderHandler(
    IApplicationDbContext dbContext,
    IMediator mediator,
    IBranchProvider branchProvider)
    : IRequestHandler<ConfirmOnlineOrderCommand, Guid>
{
    public async Task<Guid> Handle(ConfirmOnlineOrderCommand request, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.OnlineOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Online order {request.Id} not found.");

        if (order.Status != Entities.OnlineOrderStatus.Pending)
            throw new InvalidOperationException($"Only Pending orders can be confirmed (current: {order.Status}).");

        // The Sale pipeline stamps the operator's current branch; confirming
        // from another branch would move the wrong branch's stock.
        var currentBranch = branchProvider.GetRequiredBranchId();
        if (order.BranchId != currentBranch)
            throw new InvalidOperationException("Switch to the storefront's selling branch to confirm online orders.");

        var party = await dbContext.Parties
            .FirstOrDefaultAsync(p => p.Phone == order.Phone && p.IsActive
                                      && p.PartyType == Entities.PartyType.Customer, cancellationToken);
        if (party is null)
        {
            party = new Entities.Party
            {
                Id = Guid.NewGuid(),
                Title = order.CustomerName,
                Phone = order.Phone,
                Address = order.Address,
                PartyType = Entities.PartyType.Customer,
                TenantId = order.TenantId,
                BranchId = order.BranchId,
                CreatedAt = DateTime.UtcNow,
            };
            dbContext.Parties.Add(party);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // Snapshot prices are authoritative — the promotion engine is NOT
        // re-run; the customer pays exactly what the store quoted.
        var saleItems = order.Items.Select(i => new SaleDto.SaleItemRequest
        {
            ProductVariantId = i.ProductVariantId,
            Quantity = i.Quantity,
            Price = i.UnitPrice,
            Subtotal = i.LineTotal,
        }).ToList();

        var saleId = await mediator.Send(
            new CreateSaleCommand(party.Id, Discount: 0m, PaymentMethod: null, PaidAmount: null, Items: saleItems),
            cancellationToken);

        order.PartyId = party.Id;
        order.SaleId = saleId;
        order.Status = Entities.OnlineOrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return saleId;
    }
}
```

`Handlers/CommandHandlers/Ecommerce/CancelOnlineOrderHandler.cs`:
```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Ecommerce;

public class CancelOnlineOrderHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CancelOnlineOrderCommand, Unit>
{
    public async Task<Unit> Handle(CancelOnlineOrderCommand request, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.OnlineOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Online order {request.Id} not found.");

        if (order.Status != Entities.OnlineOrderStatus.Pending)
            throw new InvalidOperationException($"Only Pending orders can be cancelled (current: {order.Status}).");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("A cancel reason is required.");

        order.Status = Entities.OnlineOrderStatus.Cancelled;
        order.CancelReason = request.Reason.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
```

- [ ] **Step 4: Run to verify PASS**, then full suite.
- [ ] **Step 5: Commit** (`feat(ecommerce): confirm-to-sale and cancel flows for online orders`).

---

### Task 9: Admin online-order queries + OnlineOrderController

**Files:**
- Modify: `NextErp.Application/Queries/Ecommerce/EcommerceQueries.cs` (append)
- Create: `NextErp.Application/DTOs/Ecommerce/Responses/OnlineOrderResponses.cs`
- Create: `NextErp.Application/Handlers/QueryHandlers/Ecommerce/OnlineOrderQueryHandlers.cs`
- Create: `NextErp.API/Controllers/OnlineOrderController.cs`
- Test: `NextErp.Application.Tests/Handlers/Ecommerce/OnlineOrderQueryHandlersTests.cs`

**Interfaces:**
- Produces:
```csharp
public record GetPagedOnlineOrdersQuery(string? Status, int PageIndex = 1, int PageSize = 20) : IRequest<PagedOnlineOrdersResponse>;
public record GetOnlineOrderByIdQuery(int Id) : IRequest<OnlineOrderDetailResponse?>;

public sealed record OnlineOrderRow(int Id, string OrderNumber, string CustomerName, string Phone, int ItemCount, decimal ItemsTotal, decimal DeliveryFee, string Status, DateTime CreatedAt);
public sealed record PagedOnlineOrdersResponse(int Total, List<OnlineOrderRow> Data);
public sealed record OnlineOrderItemRow(string ProductTitle, string Sku, decimal UnitPrice, decimal Quantity, decimal LineTotal);
public sealed record OnlineOrderDetailResponse(int Id, string OrderNumber, string CustomerName, string Phone, string Address, string? Note, string Status, string? CancelReason, decimal DeliveryFee, Guid? PartyId, Guid? SaleId, DateTime CreatedAt, DateTime? ConfirmedAt, List<OnlineOrderItemRow> Items);
```
- Controller routes (all `[Authorize]`, default policy):
  - `GET api/onlineorder?status=&pageIndex=&pageSize=` → `{ total, data }`
  - `GET api/onlineorder/{id}` → detail or 404
  - `POST api/onlineorder/{id}/confirm` → `{ saleId }`
  - `POST api/onlineorder/{id}/cancel` body `{ reason }` → 204

- [ ] **Step 1: Write the failing tests**

```csharp
using NextErp.Application.Handlers.QueryHandlers.Ecommerce;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Ecommerce;

public class OnlineOrderQueryHandlersTests : HandlerTestBase
{
    private void SeedOrders()
    {
        for (var i = 1; i <= 3; i++)
        {
            Db.OnlineOrders.Add(new OnlineOrder
            {
                OrderNumber = $"W00000{i}",
                CustomerName = $"Customer {i}",
                Phone = "017",
                Address = "A",
                Status = i == 3 ? OnlineOrderStatus.Cancelled : OnlineOrderStatus.Pending,
                DeliveryFee = 60m,
                TenantId = TenantId,
                BranchId = BranchId,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                Items = { new OnlineOrderItem { ProductVariantId = 1, ProductTitle = "T", Sku = "S", UnitPrice = 100m, Quantity = 1m, LineTotal = 100m } },
            });
        }
        Db.SaveChanges();
    }

    [Fact]
    public async Task Paged_list_filters_by_status_newest_first()
    {
        SeedOrders();
        var sut = new GetPagedOnlineOrdersHandler(Db);

        var page = await sut.Handle(new GetPagedOnlineOrdersQuery("Pending"), CancellationToken.None);

        page.Total.Should().Be(2);
        page.Data.Should().HaveCount(2);
        page.Data[0].OrderNumber.Should().Be("W000001"); // newest (CreatedAt -1 min)
        page.Data[0].ItemsTotal.Should().Be(100m);
    }

    [Fact]
    public async Task Detail_returns_items_and_null_for_missing()
    {
        SeedOrders();
        var sut = new GetOnlineOrderByIdHandler(Db);
        var id = Db.OnlineOrders.First().Id;

        var detail = await sut.Handle(new GetOnlineOrderByIdQuery(id), CancellationToken.None);
        detail!.Items.Should().ContainSingle();

        (await sut.Handle(new GetOnlineOrderByIdQuery(99999), CancellationToken.None)).Should().BeNull();
    }
}
```

- [ ] **Step 2: Run to verify FAIL.**

- [ ] **Step 3: Implement**

Append to `Queries/Ecommerce/EcommerceQueries.cs` (records above). `DTOs/Ecommerce/Responses/OnlineOrderResponses.cs` (records above, namespace `NextErp.Application.DTOs.Ecommerce`).

`Handlers/QueryHandlers/Ecommerce/OnlineOrderQueryHandlers.cs`:
```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Ecommerce;

public class GetPagedOnlineOrdersHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPagedOnlineOrdersQuery, PagedOnlineOrdersResponse>
{
    public async Task<PagedOnlineOrdersResponse> Handle(GetPagedOnlineOrdersQuery request, CancellationToken cancellationToken = default)
    {
        var query = dbContext.OnlineOrders.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<OnlineOrderStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);
        var pageIndex = Math.Max(1, request.PageIndex);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var data = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OnlineOrderRow(
                o.Id, o.OrderNumber, o.CustomerName, o.Phone,
                o.Items.Count, o.Items.Sum(i => i.LineTotal), o.DeliveryFee,
                o.Status.ToString(), o.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedOnlineOrdersResponse(total, data);
    }
}

public class GetOnlineOrderByIdHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetOnlineOrderByIdQuery, OnlineOrderDetailResponse?>
{
    public async Task<OnlineOrderDetailResponse?> Handle(GetOnlineOrderByIdQuery request, CancellationToken cancellationToken = default)
    {
        return await dbContext.OnlineOrders
            .AsNoTracking()
            .Where(o => o.Id == request.Id)
            .Select(o => new OnlineOrderDetailResponse(
                o.Id, o.OrderNumber, o.CustomerName, o.Phone, o.Address, o.Note,
                o.Status.ToString(), o.CancelReason, o.DeliveryFee,
                o.PartyId, o.SaleId, o.CreatedAt, o.ConfirmedAt,
                o.Items.Select(i => new OnlineOrderItemRow(
                    i.ProductTitle, i.Sku, i.UnitPrice, i.Quantity, i.LineTotal)).ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

`NextErp.API/Controllers/OnlineOrderController.cs` (mirror `ProductController` conventions):
```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Queries.Ecommerce;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class OnlineOrderController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? status = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var page = await mediator.Send(new GetPagedOnlineOrdersQuery(status, pageIndex, pageSize));
        return Ok(new { total = page.Total, data = page.Data });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var detail = await mediator.Send(new GetOnlineOrderByIdQuery(id));
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        var saleId = await mediator.Send(new ConfirmOnlineOrderCommand(id));
        return Ok(new { saleId });
    }

    public sealed record CancelRequest(string Reason);

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelRequest body)
    {
        await mediator.Send(new CancelOnlineOrderCommand(id, body.Reason));
        return NoContent();
    }
}
```

- [ ] **Step 4: Run to verify PASS**, full suite, `dotnet build NextErp.sln -nologo` — 0 CS errors.
- [ ] **Step 5: Commit** (`feat(ecommerce): admin online-orders API`).

---

### Task 10: StoreController (public) + storefront guard + rate limiting + EcommerceController (admin publication)

**Files:**
- Create: `NextErp.API/Filters/StorefrontEnabledFilter.cs`
- Create: `NextErp.API/Controllers/StoreController.cs`
- Create: `NextErp.API/Controllers/EcommerceController.cs`
- Modify: `NextErp.API/Program.cs` (rate limiter registration + `app.UseRateLimiter()`)

**Interfaces:**
- Consumes: all Task 5–7 queries/commands; `StoreOrderCreateRequest` (with `Website` honeypot).
- Produces public routes: `GET api/store/config`, `GET api/store/categories`, `GET api/store/products`, `GET api/store/products/{id}`, `POST api/store/orders`; admin routes `GET|PUT api/ecommerce/publication`.

- [ ] **Step 1: Storefront guard filter**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NextErp.Application.Common.Settings;
using NextErp.Application.Settings;

namespace NextErp.API.Filters;

// All public store endpoints 403 while the storefront is switched off. The
// frontend maps 403 to its designed "store closed" page.
public class StorefrontEnabledFilter(ISettingsProvider settings) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ecommerce = await settings.GetAsync<EcommerceSettings>();
        if (!ecommerce.StorefrontEnabled)
        {
            context.Result = new ObjectResult(new { message = "The store is currently closed." })
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
            return;
        }
        await next();
    }
}
```

- [ ] **Step 2: StoreController**

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NextErp.API.Filters;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Queries.Ecommerce;

namespace NextErp.API.Controllers;

[AllowAnonymous]
[Route("api/store")]
[ApiController]
[EnableRateLimiting("store")]
[ServiceFilter(typeof(StorefrontEnabledFilter))]
public class StoreController(IMediator mediator) : ControllerBase
{
    [HttpGet("config")]
    public async Task<IActionResult> Config() =>
        Ok(await mediator.Send(new GetStoreConfigQuery()));

    [HttpGet("categories")]
    public async Task<IActionResult> Categories() =>
        Ok(await mediator.Send(new GetStoreCategoriesQuery()));

    [HttpGet("products")]
    public async Task<IActionResult> Products(
        [FromQuery] int? categoryId = null,
        [FromQuery] string? searchText = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 24)
    {
        var page = await mediator.Send(new GetStorePagedProductsQuery(categoryId, searchText, pageIndex, pageSize));
        return Ok(new { total = page.Total, data = page.Data });
    }

    [HttpGet("products/{id:int}")]
    public async Task<IActionResult> Product(int id)
    {
        var detail = await mediator.Send(new GetStoreProductByIdQuery(id));
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPost("orders")]
    [EnableRateLimiting("store-orders")]
    public async Task<IActionResult> CreateOrder([FromBody] StoreOrderCreateRequest request)
    {
        // Honeypot tripped: pretend success, store nothing.
        if (!string.IsNullOrEmpty(request.Website))
            return Ok(new { orderNumber = "W000000" });

        var orderNumber = await mediator.Send(new CreateOnlineOrderCommand(
            request.CustomerName, request.Phone, request.Address, request.Note, request.Items));
        return Ok(new { orderNumber });
    }
}
```

- [ ] **Step 3: EcommerceController (admin)**

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Queries.Ecommerce;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class EcommerceController(IMediator mediator) : ControllerBase
{
    [HttpGet("publication")]
    public async Task<IActionResult> GetPublication() =>
        Ok(await mediator.Send(new GetEcommercePublicationQuery()));

    [HttpPut("publication")]
    public async Task<IActionResult> SetPublication([FromBody] SetEcommercePublicationCommand command)
    {
        await mediator.Send(command);
        return NoContent();
    }
}
```

- [ ] **Step 4: Program.cs wiring**

Register the filter + rate limiter near the caching registrations:
```csharp
builder.Services.AddScoped<NextErp.API.Filters.StorefrontEnabledFilter>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("store", httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
            }));
    options.AddPolicy("store-orders", httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
            }));
});
```
Add `app.UseRateLimiter();` immediately after `app.UseRouting();` (line ~310, before `app.UseCors`). Add `using Microsoft.AspNetCore.RateLimiting;` if needed.

- [ ] **Step 5: Build + smoke**

```powershell
dotnet build NextErp.sln -nologo   # 0 CS errors
dotnet test NextErp.Application.Tests/NextErp.Application.Tests.csproj --nologo  # all green
```
Then run the API and verify manually (storefront disabled by default):
```powershell
# with the API running:
curl http://localhost:5039/api/store/config          # expect 403 { message: "The store is currently closed." }
```
Flip `StorefrontEnabled` via the settings API (or SQL) and re-check `config` returns 200 with defaults. Stop the API afterwards.

- [ ] **Step 6: Commit** (`feat(ecommerce): public store API with rate limiting, honeypot and storefront guard`).

---

### Task 11: Full verification + push

- [ ] **Step 1:** `dotnet test NextErp.Application.Tests/NextErp.Application.Tests.csproj --nologo` — expected: **all tests pass, 0 failures** (277 pre-existing + ~17 new).
- [ ] **Step 2:** `dotnet build NextErp.sln -nologo` — 0 CS errors (API must not be running).
- [ ] **Step 3:** Review `git log --oneline` — one commit per task, all with the Co-Authored-By footer.
- [ ] **Step 4:** `git push origin main`.

---

## Plan self-review notes (already applied)

- **Spec coverage:** flags ✓ (T1), settings ✓ (T2), OnlineOrder ✓ (T3), order number ✓ (T4), publication admin ✓ (T5), public catalog + availability rule ✓ (T6), order intake + validation + notification ✓ (T7), confirm/cancel + party match + snapshot prices + branch guard ✓ (T8), admin API ✓ (T9), public controller + 403 guard + rate limit + honeypot ✓ (T10). Plans 2–3 cover the ERP UI and storefront.
- **Type consistency:** `StoreOrderItemRequest` used by both DTO and command; `OnlineOrderStatus` string over the wire via global converter; availability rule shared via `StoreQueryShared.LowStock`.
- **Known execution checkpoints (verify, don't assume):** exact construction helper in `SettingsProviderTests` (T2 step 1); whether `ApplicationDbContext` applies configurations by assembly scan (T3 step 4); SQLite translation of the nested stock sum (T6 step 3 note); `SimpleProductVariantFactory.CreateDefault` tenant/branch stamping (T7 test seed).
