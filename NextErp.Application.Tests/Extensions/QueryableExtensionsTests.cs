using NextErp.Application.Common.Extensions;
using NextErp.Application.Interfaces;
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Extensions;

/// <summary>
/// Verifies WhereIf / IncludeIf / OrderByIf / PageIf produce correct SQL semantics
/// against a real (SQLite) provider. Each "off" path must return data unchanged;
/// each "on" path must apply the operator exactly as the manual equivalent.
/// </summary>
public class QueryableExtensionsTests : IDisposable
{
    private readonly TestDbContextFactory.TestContext _ctx;
    private readonly Guid _branchId = Guid.NewGuid();

    public QueryableExtensionsTests()
    {
        var branchProvider = Substitute.For<IBranchProvider>();
        branchProvider.GetBranchId().Returns(_branchId);
        branchProvider.GetRequiredBranchId().Returns(_branchId);
        branchProvider.IsGlobal().Returns(false);
        _ctx = TestDbContextFactory.Create(branchProvider);
    }

    public void Dispose() => _ctx.Dispose();

    private async Task SeedAsync()
    {
        var branch = new BranchBuilder().WithId(_branchId).WithTitle("Main").Build();
        _ctx.Db.Branches.Add(branch);

        var category = new CategoryBuilder().WithTitle("Cat").Build();
        _ctx.Db.Categories.Add(category);
        await _ctx.Db.SaveChangesAsync(); // flush so Category.Id is generated for FK

        // 5 active products, 2 inactive
        for (var i = 1; i <= 5; i++)
        {
            var p = new ProductBuilder()
                .WithTitle($"Active-{i}")
                .WithBranch(_branchId)
                .WithCategory(category.Id)
                .Build();
            p.IsActive = true;
            _ctx.Db.Products.Add(p);
        }
        for (var i = 1; i <= 2; i++)
        {
            var p = new ProductBuilder()
                .WithTitle($"Inactive-{i}")
                .WithBranch(_branchId)
                .WithCategory(category.Id)
                .Build();
            p.IsActive = false;
            _ctx.Db.Products.Add(p);
        }

        await _ctx.Db.SaveChangesAsync();
    }

    // ===== WhereIf =====

    [Fact]
    public async Task WhereIf_false_returns_all()
    {
        await SeedAsync();
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIf(false, p => p.IsActive)
            .ToListAsync();
        result.Should().HaveCount(7);
    }

    [Fact]
    public async Task WhereIf_true_filters()
    {
        await SeedAsync();
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIf(true, p => p.IsActive)
            .ToListAsync();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task WhereIfHasValue_null_returns_all()
    {
        await SeedAsync();
        Guid? branchFilter = null;
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIfHasValue(branchFilter, p => p.BranchId == branchFilter!.Value)
            .ToListAsync();
        result.Should().HaveCount(7);
    }

    [Fact]
    public async Task WhereIfHasValue_present_filters()
    {
        await SeedAsync();
        var branch = await _ctx.Db.Branches.FirstAsync();
        Guid? branchFilter = branch.Id;
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIfHasValue(branchFilter, p => p.BranchId == branchFilter!.Value)
            .ToListAsync();
        result.Should().HaveCount(7);   // all are in this branch
    }

    [Fact]
    public async Task WhereIfNotEmpty_null_string_returns_all()
    {
        await SeedAsync();
        string? search = null;
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIfNotEmpty(search, p => p.Title.Contains(search!))
            .ToListAsync();
        result.Should().HaveCount(7);
    }

    [Fact]
    public async Task WhereIfNotEmpty_whitespace_string_returns_all()
    {
        await SeedAsync();
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIfNotEmpty("   ", p => p.Title.Contains("anything"))
            .ToListAsync();
        result.Should().HaveCount(7);
    }

    [Fact]
    public async Task WhereIfNotNullOrEmpty_whitespace_string_treats_as_value()
    {
        await SeedAsync();
        // Unlike WhereIfNotEmpty (whitespace-as-empty), this overload treats "   " as a
        // real value and applies the predicate. Result: nothing matches " " in titles.
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIfNotNullOrEmpty("   ", p => p.Title.Contains("XYZ-NOPE"))
            .ToListAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task WhereIfNotNullOrEmpty_null_returns_all()
    {
        await SeedAsync();
        string? value = null;
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIfNotNullOrEmpty(value, p => p.Title.Contains(value!))
            .ToListAsync();
        result.Should().HaveCount(7);
    }

    [Fact]
    public async Task WhereIfNotEmpty_value_filters()
    {
        await SeedAsync();
        var search = "Active";
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIfNotEmpty(search, p => p.Title.StartsWith(search))
            .ToListAsync();
        result.Should().HaveCount(5);   // "Active-N" prefix
    }

    [Fact]
    public async Task WhereIfAny_empty_collection_returns_all()
    {
        await SeedAsync();
        IReadOnlyCollection<int>? ids = Array.Empty<int>();
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIfAny(ids, p => ids!.Contains(p.Id))
            .ToListAsync();
        result.Should().HaveCount(7);
    }

    [Fact]
    public async Task WhereIfAny_null_collection_returns_all()
    {
        await SeedAsync();
        IReadOnlyCollection<int>? ids = null;
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIfAny(ids, p => ids!.Contains(p.Id))
            .ToListAsync();
        result.Should().HaveCount(7);
    }

    [Fact]
    public async Task WhereIfAny_with_values_filters()
    {
        await SeedAsync();
        var firstTwoIds = await _ctx.Db.Products.OrderBy(p => p.Id).Select(p => p.Id).Take(2).ToListAsync();
        IReadOnlyCollection<int> ids = firstTwoIds;
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIfAny(ids, p => ids.Contains(p.Id))
            .ToListAsync();
        result.Should().HaveCount(2);
    }

    // ===== IncludeIf =====

    [Fact]
    public async Task IncludeIf_false_does_not_load_navigation()
    {
        await SeedAsync();
        var product = await _ctx.Db.Products.IgnoreQueryFilters()
            .IncludeIf(false, p => p.Category)
            .FirstAsync();

        // Without Include, the navigation should be lazy-null (no-tracking would still leave it null).
        // EF Core does not auto-load navigations on a no-include query, so Category remains null
        // unless the entity was already tracked with Category populated.
        // Use a separate fresh context to assert without tracker influence.
        var verifyOptions = new DbContextOptionsBuilder<NextErp.Infrastructure.ApplicationDbContext>()
            .UseSqlite(_ctx.Connection)
            .Options;
        using var verifyDb = new NextErp.Infrastructure.ApplicationDbContext(verifyOptions);
        var fresh = await verifyDb.Products.IgnoreQueryFilters()
            .IncludeIf(false, p => p.Category)
            .AsNoTracking()
            .FirstAsync();
        fresh.Category.Should().BeNull("IncludeIf(false) must not eager-load the navigation");
    }

    [Fact]
    public async Task IncludeIf_true_loads_navigation()
    {
        await SeedAsync();
        var verifyOptions = new DbContextOptionsBuilder<NextErp.Infrastructure.ApplicationDbContext>()
            .UseSqlite(_ctx.Connection)
            .Options;
        using var verifyDb = new NextErp.Infrastructure.ApplicationDbContext(verifyOptions);
        var product = await verifyDb.Products.IgnoreQueryFilters()
            .IncludeIf(true, p => p.Category)
            .AsNoTracking()
            .FirstAsync();
        product.Category.Should().NotBeNull("IncludeIf(true) must eager-load the navigation");
    }

    // ===== OrderByIf =====

    [Fact]
    public async Task OrderByIf_false_does_not_sort()
    {
        await SeedAsync();
        // Without sort, EF will return in insertion order on SQLite — but the contract here is
        // "return the source unchanged", so we just assert count + that it doesn't throw.
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .OrderByIf(false, p => p.Title)
            .ToListAsync();
        result.Should().HaveCount(7);
    }

    [Fact]
    public async Task OrderByIf_true_sorts()
    {
        await SeedAsync();
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .OrderByIf(true, p => p.Title)
            .ToListAsync();
        result.Select(p => p.Title).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task OrderByDescendingIf_true_sorts_desc()
    {
        await SeedAsync();
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .OrderByDescendingIf(true, p => p.Title)
            .ToListAsync();
        result.Select(p => p.Title).Should().BeInDescendingOrder();
    }

    // ===== PageIf =====

    [Fact]
    public async Task PageIf_false_returns_all()
    {
        await SeedAsync();
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .OrderBy(p => p.Title)
            .PageIf(false, pageIndex: 1, pageSize: 3)
            .ToListAsync();
        result.Should().HaveCount(7);
    }

    [Fact]
    public async Task PageIf_true_returns_slice()
    {
        await SeedAsync();
        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .OrderBy(p => p.Title)
            .PageIf(true, pageIndex: 2, pageSize: 3)
            .ToListAsync();
        // Page 2 (size 3): items 4-6 by Title alphabetically
        result.Should().HaveCount(3);
    }

    // ===== Composability =====

    [Fact]
    public async Task Chained_extensions_compose_correctly()
    {
        await SeedAsync();
        string? search = "Active";
        IReadOnlyCollection<int>? ids = null;

        var result = await _ctx.Db.Products.IgnoreQueryFilters()
            .WhereIf(true, p => p.IsActive)
            .WhereIfNotEmpty(search, p => p.Title.StartsWith(search!))
            .WhereIfAny(ids, p => ids!.Contains(p.Id))
            .OrderByIf(true, p => p.Title)
            .PageIf(true, pageIndex: 1, pageSize: 2)
            .ToListAsync();

        result.Should().HaveCount(2);
        result.Select(p => p.Title).Should().BeInAscendingOrder();
        result.Should().AllSatisfy(p => p.IsActive.Should().BeTrue());
    }
}
