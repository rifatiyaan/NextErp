using AutoMapper;
using NextErp.Application.Handlers.QueryHandlers.Product;
using NextErp.Application.Queries;
using NSubstitute;

namespace NextErp.Application.Tests.Handlers.Product;

public class GetPagedProductsHandlerTests : HandlerTestBase
{
    private static readonly IMapper Mapper = BuildMapper();

    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
        {
            // Pull every Profile in the Application assembly so the Product map can resolve
            // dependent maps (Category, ProductVariation, etc.) without manual registration.
            c.AddMaps(typeof(NextErp.Application.Mappings.ProductProfile).Assembly);
        });
        return cfg.CreateMapper();
    }

    private GetPagedProductsHandler BuildHandler() => new(Db, BranchProvider, Mapper);

    private async Task SeedSchemaAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Empty_database_returns_zero_total_and_empty_records()
    {
        await SeedSchemaAsync();
        var sut = BuildHandler();

        var result = await sut.Handle(new GetPagedProductsQuery(), CancellationToken.None);

        result.Total.Should().Be(0);
        result.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task Pagination_returns_correct_slice_and_total()
    {
        await SeedSchemaAsync();
        for (int i = 1; i <= 15; i++)
        {
            Db.Products.Add(new ProductBuilder()
                .WithId(i)
                .WithTitle($"Product {i:D2}")
                .WithCode($"P{i:D3}")
                .WithCategory(1)
                .WithTenant(TenantId)
                .WithBranch(BranchId)
                .CreatedAt(DateTime.UtcNow.AddMinutes(-i)) // i=1 newest, i=15 oldest
                .Build());
        }
        await Db.SaveChangesAsync();
        var sut = BuildHandler();

        var result = await sut.Handle(new GetPagedProductsQuery(PageIndex: 2, PageSize: 5), CancellationToken.None);

        result.Total.Should().Be(15);
        result.Records.Should().HaveCount(5);
    }

    [Fact]
    public async Task Branch_filter_excludes_other_branches_for_non_global_user()
    {
        // BranchProvider defaults to non-global, BranchId = our branch
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        var otherBranchId = Guid.NewGuid();
        Db.Branches.Add(new BranchBuilder().WithId(otherBranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(1).WithTenant(TenantId).WithBranch(BranchId).Build());

        for (int i = 1; i <= 5; i++)
        {
            Db.Products.Add(new ProductBuilder()
                .WithId(i).WithCategory(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        }
        for (int i = 100; i < 105; i++)
        {
            Db.Products.Add(new ProductBuilder()
                .WithId(i).WithCategory(1).WithTenant(TenantId).WithBranch(otherBranchId).Build());
        }
        await Db.SaveChangesAsync();
        var sut = BuildHandler();

        var result = await sut.Handle(new GetPagedProductsQuery(PageSize: 50), CancellationToken.None);

        result.Total.Should().Be(5);
        result.Records.Should().AllSatisfy(p => p.BranchId.Should().Be(BranchId));
    }

    [Fact]
    public async Task Stock_aggregate_sums_variant_quantities_and_flags_low_stock()
    {
        await SeedSchemaAsync();
        Db.Products.Add(new ProductBuilder()
            .WithId(1).WithCategory(1).WithTenant(TenantId).WithBranch(BranchId).Build());
        // Two variants, 3 + 7 = 10 total available
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(1).WithProduct(1).WithSku("V1").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(2).WithProduct(1).WithSku("V2").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Stocks.Add(new StockBuilder()
            .WithVariant(1).WithAvailable(3m).WithReorderLevel(5m)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Stocks.Add(new StockBuilder()
            .WithVariant(2).WithAvailable(7m).WithReorderLevel(5m)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var result = await sut.Handle(new GetPagedProductsQuery(), CancellationToken.None);

        result.Total.Should().Be(1);
        var dto = result.Records.Single();
        dto.TotalAvailableQuantity.Should().Be(10m);
        // Variant 1 has 3 available with reorder level 5 -> low stock present
        dto.HasLowStock.Should().Be(true);
    }
}
