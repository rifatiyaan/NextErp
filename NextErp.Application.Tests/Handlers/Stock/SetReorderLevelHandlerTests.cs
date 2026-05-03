using NextErp.Application.Commands.Stock;
using NextErp.Application.Handlers.CommandHandlers.Stock;
using NSubstitute;

namespace NextErp.Application.Tests.Handlers.Stock;

public class SetReorderLevelHandlerTests : HandlerTestBase
{
    private const int VariantId = 1;

    private SetReorderLevelHandler BuildHandler() => new(Db, BranchProvider);

    private async Task SeedVariantWithStockAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(1).WithCategory(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantId).WithProduct(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Stocks.Add(new StockBuilder()
            .WithVariant(VariantId).WithAvailable(10m).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Happy_path_sets_reorder_level_and_flushes()
    {
        await SeedVariantWithStockAsync();
        var before = DateTime.UtcNow;
        var sut = BuildHandler();

        await sut.Handle(new SetReorderLevelCommand(VariantId, 5m), CancellationToken.None);

        var stock = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantId);
        stock.ReorderLevel.Should().Be(5m);
        stock.UpdatedAt.Should().NotBeNull();
        stock.UpdatedAt!.Value.Should().BeOnOrAfter(before).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Stock_row_missing_in_branch_throws_no_stock_record()
    {
        // Variant exists, but no Stock row in current branch.
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(1).WithCategory(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantId).WithProduct(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();

        var sut = BuildHandler();

        var act = async () => await sut.Handle(new SetReorderLevelCommand(VariantId, 3m), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*No stock record*");
    }

    [Fact]
    public async Task Branch_context_required_when_provider_returns_null()
    {
        await SeedVariantWithStockAsync();
        // Override default: simulate a missing branch claim. GetRequiredBranchId is the
        // path SetReorderLevelHandler hits — with no branch the implementation throws.
        BranchProvider.GetRequiredBranchId().Returns(_ => throw new InvalidOperationException("Branch context required."));

        var sut = BuildHandler();

        var act = async () => await sut.Handle(new SetReorderLevelCommand(VariantId, 5m), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Null_reorder_level_clears_threshold()
    {
        await SeedVariantWithStockAsync();
        // First set a value so we can prove null actually clears it.
        var stock = await Db.Stocks.FirstAsync(s => s.ProductVariantId == VariantId);
        stock.ReorderLevel = 5m;
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        await sut.Handle(new SetReorderLevelCommand(VariantId, ReorderLevel: null), CancellationToken.None);

        var fresh = await Db.Stocks.AsNoTracking().FirstAsync(s => s.ProductVariantId == VariantId);
        fresh.ReorderLevel.Should().BeNull();
    }
}

