using NextErp.Application.Handlers.QueryHandlers.Stock;
using NextErp.Application.Queries;

namespace NextErp.Application.Tests.Handlers.Stock;

public class GetLowStockReportHandlerTests : HandlerTestBase
{
    private GetLowStockReportHandler BuildHandler() => new(Db);

    private async Task SeedSchemaAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(400).WithTitle("Cat").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(400).WithCategory(400).WithCode("LP-400").WithTitle("Low Stock Product")
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
    }

    private void AddVariantStock(int variantId, decimal available, decimal? reorderLevel, string sku)
    {
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(variantId).WithProduct(400).WithSku(sku)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        var builder = new StockBuilder()
            .WithVariant(variantId).WithAvailable(available)
            .WithTenant(TenantId).WithBranch(BranchId);
        if (reorderLevel.HasValue)
            builder = builder.WithReorderLevel(reorderLevel.Value);
        Db.Stocks.Add(builder.Build());
    }

    [Fact]
    public async Task Items_at_or_below_reorder_level_included_others_excluded()
    {
        await SeedSchemaAsync();

        // Reorder = 10. Available 5 (≤10) — included.
        AddVariantStock(401, available: 5m, reorderLevel: 10m, sku: "SKU-401");
        // Reorder = 10. Available 15 (>10) — excluded.
        AddVariantStock(402, available: 15m, reorderLevel: 10m, sku: "SKU-402");
        // Reorder = 10. Available exactly 10 (=10) — included.
        AddVariantStock(403, available: 10m, reorderLevel: 10m, sku: "SKU-403");
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var report = await sut.Handle(new GetLowStockReportQuery(), CancellationToken.None);

        report.Items.Select(i => i.VariantSku).Should().BeEquivalentTo(new[] { "SKU-401", "SKU-403" });
        report.TotalLowStockVariants.Should().Be(2);
    }

    [Fact]
    public async Task Status_reflects_availability_thresholds()
    {
        await SeedSchemaAsync();

        // Out of Stock: available = 0.
        AddVariantStock(411, available: 0m, reorderLevel: 10m, sku: "OOS");
        // Critical: available 5 ≤ ReorderLevel(10) × 0.5 = 5.
        AddVariantStock(412, available: 5m, reorderLevel: 10m, sku: "CRIT");
        // Low: available 8, > 5 (= 10*0.5) but ≤ 10.
        AddVariantStock(413, available: 8m, reorderLevel: 10m, sku: "LOW");
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var report = await sut.Handle(new GetLowStockReportQuery(), CancellationToken.None);

        report.Items.Single(i => i.VariantSku == "OOS").Status.Should().Be("Out of Stock");
        report.Items.Single(i => i.VariantSku == "CRIT").Status.Should().Be("Critical");
        report.Items.Single(i => i.VariantSku == "LOW").Status.Should().Be("Low");
    }

    [Fact]
    public async Task Reorder_level_null_uses_default_threshold_of_ten()
    {
        await SeedSchemaAsync();

        // Reorder = null → fallback threshold = 10. Available = 7 ≤ 10 → included.
        AddVariantStock(421, available: 7m, reorderLevel: null, sku: "NULL-7");
        // Reorder = null. Available = 11 > 10 → excluded.
        AddVariantStock(422, available: 11m, reorderLevel: null, sku: "NULL-11");
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var report = await sut.Handle(new GetLowStockReportQuery(), CancellationToken.None);

        report.Items.Should().ContainSingle(i => i.VariantSku == "NULL-7");
        report.Items.Should().NotContain(i => i.VariantSku == "NULL-11");
    }
}

