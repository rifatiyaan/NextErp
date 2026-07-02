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
