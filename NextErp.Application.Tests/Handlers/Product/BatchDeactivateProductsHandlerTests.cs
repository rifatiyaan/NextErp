using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Product;

namespace NextErp.Application.Tests.Handlers.Product;

public class BatchDeactivateProductsHandlerTests : HandlerTestBase
{
    private const int CategoryId = 50;

    private BatchDeactivateProductsHandler BuildHandler() => new(Db);

    private async Task SeedAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(CategoryId).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
    }

    private async Task SeedActiveProductsAsync(params int[] ids)
    {
        foreach (var id in ids)
        {
            Db.Products.Add(new ProductBuilder()
                .WithId(id).WithCategory(CategoryId)
                .WithTenant(TenantId).WithBranch(BranchId).Build());
        }
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Happy_path_deactivates_all_active_rows()
    {
        await SeedAsync();
        await SeedActiveProductsAsync(401, 402, 403);
        var before = DateTime.UtcNow;
        var sut = BuildHandler();

        var count = await sut.Handle(
            new BatchDeactivateProductsCommand(new[] { 401, 402, 403 }),
            CancellationToken.None);

        count.Should().Be(3);
        // After deactivate IsActive=false rows are filtered out by the global branch
        // filter (which AND-s `IsActive`), so verification must bypass query filters.
        var rows = await Db.Products.AsNoTracking()
            .IgnoreQueryFilters()
            .Where(p => new[] { 401, 402, 403 }.Contains(p.Id)).ToListAsync();
        rows.Should().HaveCount(3);
        rows.Should().OnlyContain(p => p.IsActive == false);
        rows.Should().OnlyContain(p =>
            p.UpdatedAt != null && p.UpdatedAt!.Value >= before);
    }

    [Fact]
    public async Task Mixed_active_and_inactive_returns_active_count()
    {
        await SeedAsync();
        Db.Products.Add(new ProductBuilder()
            .WithId(501).WithCategory(CategoryId)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(502).WithCategory(CategoryId).Inactive()
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(503).WithCategory(CategoryId)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var count = await sut.Handle(
            new BatchDeactivateProductsCommand(new[] { 501, 502, 503 }),
            CancellationToken.None);

        // 502 was already inactive before the call; only 501 + 503 flip.
        count.Should().Be(2);
        var rows = await Db.Products.AsNoTracking()
            .IgnoreQueryFilters()
            .Where(p => new[] { 501, 502, 503 }.Contains(p.Id))
            .OrderBy(p => p.Id).ToListAsync();
        rows.Should().HaveCount(3);
        rows.Should().OnlyContain(p => p.IsActive == false);
    }

    [Fact]
    public async Task Unknown_ids_yield_zero_count()
    {
        await SeedAsync();
        await SeedActiveProductsAsync(601);
        var sut = BuildHandler();

        var count = await sut.Handle(
            new BatchDeactivateProductsCommand(new[] { 9001, 9002 }),
            CancellationToken.None);

        count.Should().Be(0);
        var fresh = await Db.Products.AsNoTracking().FirstAsync(p => p.Id == 601);
        fresh.IsActive.Should().BeTrue();
    }
}
