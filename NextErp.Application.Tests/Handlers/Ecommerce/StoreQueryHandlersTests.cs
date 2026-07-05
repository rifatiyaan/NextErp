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
    private const int PricedCat = 720;

    private ISettingsProvider SettingsWith(Guid sellingBranchId, bool enableBranchSelling = false) =>
        SettingsProviderReturning(new EcommerceSettings
        {
            StorefrontEnabled = true,
            EnableBranchSelling = enableBranchSelling,
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
    public async Task Products_in_another_branch_are_excluded_when_branch_selling_is_on()
    {
        await SeedCatalogAsync();
        var otherBranch = Guid.NewGuid();
        Db.Branches.Add(new BranchBuilder().WithId(otherBranch).WithTenant(TenantId).Build());
        await Db.SaveChangesAsync();
        var sut = new GetStorePagedProductsHandler(Db, SettingsWith(otherBranch, enableBranchSelling: true));

        var page = await sut.Handle(new GetStorePagedProductsQuery(null, null), CancellationToken.None);

        page.Total.Should().Be(0);
    }

    [Fact]
    public async Task Products_auto_resolve_to_the_default_branch_when_branch_selling_is_off()
    {
        await SeedCatalogAsync();
        // Branch selling OFF + a garbage SellingBranchId ("1") must not matter:
        // the store auto-targets the only active branch (the fragile Guid.Empty
        // luck is gone). This is the single-branch, zero-config path.
        var settings = SettingsProviderReturning(new EcommerceSettings
        {
            StorefrontEnabled = true,
            EnableBranchSelling = false,
            SellingBranchId = "1",
        });
        var sut = new GetStorePagedProductsHandler(Db, settings);

        var page = await sut.Handle(new GetStorePagedProductsQuery(null, null), CancellationToken.None);

        page.Total.Should().Be(1);
        page.Data.Should().ContainSingle(p => p.Title == "Visible");
    }

    [Fact]
    public async Task Products_fall_back_to_default_branch_when_configured_branch_is_missing()
    {
        await SeedCatalogAsync();
        // Flag ON but SellingBranchId points to a branch that does not exist ->
        // fall back to the default branch so a misconfig can't break the store.
        var sut = new GetStorePagedProductsHandler(Db, SettingsWith(Guid.NewGuid(), enableBranchSelling: true));

        var page = await sut.Handle(new GetStorePagedProductsQuery(null, null), CancellationToken.None);

        page.Total.Should().Be(1);
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

    // V2-3: three published products at 50/100/200 in one published category.
    private async Task SeedPricedAsync()
    {
        if (!Db.Branches.Local.Any(b => b.Id == BranchId) && !Db.Branches.Any(b => b.Id == BranchId))
            Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());

        var cat = new CategoryBuilder().WithId(PricedCat).WithTitle("Priced").WithTenant(TenantId).WithBranch(BranchId).Build();
        cat.IsPublishedOnline = true;
        Db.Categories.Add(cat);

        foreach (var (id, price) in new[] { (7200, 50m), (7201, 100m), (7202, 200m) })
        {
            var p = new ProductBuilder().WithId(id).WithTitle($"P{id}").WithCode($"C{id}").WithPrice(price)
                .WithCategory(PricedCat).WithTenant(TenantId).WithBranch(BranchId).Build();
            p.IsPublishedOnline = true;
            Db.Products.Add(p);
        }

        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Products_filters_by_inclusive_price_range()
    {
        await SeedPricedAsync();
        var sut = new GetStorePagedProductsHandler(Db, SettingsWith(BranchId));

        var page = await sut.Handle(
            new GetStorePagedProductsQuery(PricedCat, null, MinPrice: 75m, MaxPrice: 150m), CancellationToken.None);

        page.Total.Should().Be(1);
        page.Data.Should().ContainSingle(p => p.Price == 100m);
    }

    [Fact]
    public async Task Products_min_price_bound_is_inclusive()
    {
        await SeedPricedAsync();
        var sut = new GetStorePagedProductsHandler(Db, SettingsWith(BranchId));

        var page = await sut.Handle(
            new GetStorePagedProductsQuery(PricedCat, null, MinPrice: 100m), CancellationToken.None);

        page.Total.Should().Be(2);
        page.Data.Select(p => p.Price).Should().BeEquivalentTo(new[] { 100m, 200m });
    }

    [Fact]
    public async Task PriceRange_returns_min_and_max_for_a_category()
    {
        await SeedPricedAsync();
        var sut = new GetStorePriceRangeHandler(Db, SettingsWith(BranchId));

        var range = await sut.Handle(new GetStorePriceRangeQuery(PricedCat), CancellationToken.None);

        range.Min.Should().Be(50m);
        range.Max.Should().Be(200m);
    }

    [Fact]
    public async Task PriceRange_is_zero_when_no_products_are_visible()
    {
        var sut = new GetStorePriceRangeHandler(Db, SettingsWith(BranchId));

        var range = await sut.Handle(new GetStorePriceRangeQuery(), CancellationToken.None);

        range.Min.Should().Be(0m);
        range.Max.Should().Be(0m);
    }
}
