using NextErp.Application.Handlers.QueryHandlers.Stock;
using NextErp.Application.Queries;

namespace NextErp.Application.Tests.Handlers.Stock;

public class GetCurrentStockReportHandlerTests : HandlerTestBase
{
    private GetCurrentStockReportHandler BuildHandler() => new(Db);

    private async Task SeedAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder()
            .WithId(600).WithTitle("Cat").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(600).WithCategory(600).WithCode("P-600").WithTitle("Reportable Product")
            .WithTenant(TenantId).WithBranch(BranchId).Build());

        // Two variants with stock.
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(601).WithProduct(600).WithSku("V-601").WithTitle("Small")
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(602).WithProduct(600).WithSku("V-602").WithTitle("Large")
            .WithTenant(TenantId).WithBranch(BranchId).Build());

        Db.Stocks.Add(new StockBuilder()
            .WithVariant(601).WithAvailable(7m).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Stocks.Add(new StockBuilder()
            .WithVariant(602).WithAvailable(3m).WithTenant(TenantId).WithBranch(BranchId).Build());

        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task All_stocks_listed_with_variant_and_product_info()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var report = await sut.Handle(new GetCurrentStockReportQuery(), CancellationToken.None);

        report.Stocks.Should().HaveCount(2);
        report.TotalVariants.Should().Be(2);
        report.Stocks.Should().Contain(s =>
            s.VariantSku == "V-601" && s.ProductCode == "P-600" && s.ProductTitle == "Reportable Product");
        report.Stocks.Should().Contain(s =>
            s.VariantSku == "V-602" && s.ProductCode == "P-600" && s.ProductTitle == "Reportable Product");
    }

    [Fact]
    public async Task Total_quantity_aggregates_across_variants()
    {
        await SeedAsync();
        var sut = BuildHandler();

        var report = await sut.Handle(new GetCurrentStockReportQuery(), CancellationToken.None);

        report.TotalQuantity.Should().Be(10m); // 7 + 3
    }
}

