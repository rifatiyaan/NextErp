using NextErp.Application.Interfaces;
using NextErp.Application.Services;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Services;

public class PricingServiceBogoTests : HandlerTestBase
{
    private const int ProdA = 100;      // primary buy + same-product reward (e.g. Coca Cola)
    private const int ProdB = 200;      // cross-product reward (e.g. Chips)
    private const int CatDrinks = 9;
    private const int VarA = 1001;
    private const int VarB = 1002;

    private PricingService BuildSut() => new(Db);

    private async Task SeedCatalogAsync(decimal priceA = 40m, decimal priceB = 10m)
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(CatDrinks).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder().WithId(ProdA).WithCategory(CatDrinks).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder().WithId(ProdB).WithCategory(CatDrinks).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VarA).WithProduct(ProdA).WithSku("A").WithPrice(priceA).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VarB).WithProduct(ProdB).WithSku("B").WithPrice(priceB).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
    }

    private async Task<Guid> SeedBogoAsync(
        List<int>? buyProducts = null,
        List<int>? buyCategories = null,
        List<int>? getProducts = null,
        int buyN = 3,
        int getN = 1,
        decimal pct = 100m,
        decimal? maxReward = null)
    {
        var id = Guid.NewGuid();
        Db.Promotions.Add(new Promotion
        {
            Id = id,
            Name = "BOGO",
            Type = PromotionType.Bogo,
            Config = new PromotionConfig
            {
                BuyQuantity = buyN,
                GetQuantity = getN,
                GetDiscountPercent = pct,
                BuyProductIds = buyProducts,
                BuyCategoryIds = buyCategories,
                GetProductIds = getProducts,
                MaxRewardQuantity = maxReward,
            },
            IsActive = true,
            Stackable = true,
            TenantId = TenantId,
            CreatedAt = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();
        return id;
    }

    private static PricingLine Line(int variantId, int productId, decimal qty, decimal price, int categoryId = 0) =>
        new(variantId, productId, categoryId, qty, price, ManualDiscount: 0m);

    [Fact]
    public async Task Same_product_buy_3_get_1_emits_one_free_bonus()
    {
        await SeedCatalogAsync();
        await SeedBogoAsync(buyProducts: new() { ProdA }, getProducts: new() { ProdA }, buyN: 3, getN: 1);
        var sut = BuildSut();

        var result = await sut.ResolveForSaleAsync(
            new List<PricingLine> { Line(VarA, ProdA, qty: 3m, price: 40m) },
            null, DateTime.UtcNow);

        result.BonusItems.Should().ContainSingle();
        var b = result.BonusItems[0];
        b.ProductVariantId.Should().Be(VarA);
        b.Quantity.Should().Be(1m);
        b.DiscountPercent.Should().Be(100m);
    }

    [Fact]
    public async Task Below_buy_threshold_emits_no_bonus()
    {
        await SeedCatalogAsync();
        await SeedBogoAsync(buyProducts: new() { ProdA }, getProducts: new() { ProdA }, buyN: 3, getN: 1);
        var sut = BuildSut();

        var result = await sut.ResolveForSaleAsync(
            new List<PricingLine> { Line(VarA, ProdA, qty: 2m, price: 40m) },
            null, DateTime.UtcNow);

        result.BonusItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Two_full_sets_emit_two_bonus_units()
    {
        await SeedCatalogAsync();
        await SeedBogoAsync(buyProducts: new() { ProdA }, getProducts: new() { ProdA }, buyN: 3, getN: 1);
        var sut = BuildSut();

        var result = await sut.ResolveForSaleAsync(
            new List<PricingLine> { Line(VarA, ProdA, qty: 6m, price: 40m) },
            null, DateTime.UtcNow);

        result.BonusItems.Single().Quantity.Should().Be(2m);
    }

    [Fact]
    public async Task Cross_product_auto_adds_reward_even_when_not_in_cart()
    {
        // The bug this fixes: buy Coca Cola, get Chips free — Chips is NOT in
        // the cart, but should be auto-added as a bonus line.
        await SeedCatalogAsync();
        await SeedBogoAsync(buyProducts: new() { ProdA }, getProducts: new() { ProdB }, buyN: 3, getN: 1);
        var sut = BuildSut();

        var result = await sut.ResolveForSaleAsync(
            new List<PricingLine> { Line(VarA, ProdA, qty: 3m, price: 40m) },
            null, DateTime.UtcNow);

        result.BonusItems.Should().ContainSingle();
        result.BonusItems[0].ProductVariantId.Should().Be(VarB);
        result.BonusItems[0].Quantity.Should().Be(1m);
        result.BonusItems[0].UnitPrice.Should().Be(10m); // Chips catalog price
    }

    [Fact]
    public async Task Category_buy_set_qualifies_the_cart()
    {
        await SeedCatalogAsync();
        await SeedBogoAsync(buyCategories: new() { CatDrinks }, getProducts: new() { ProdB }, buyN: 3, getN: 1);
        var sut = BuildSut();

        var result = await sut.ResolveForSaleAsync(
            new List<PricingLine> { Line(VarA, ProdA, qty: 3m, price: 40m, categoryId: CatDrinks) },
            null, DateTime.UtcNow);

        result.BonusItems.Single().ProductVariantId.Should().Be(VarB);
    }

    [Fact]
    public async Task Multiple_reward_products_each_emit_a_bonus()
    {
        await SeedCatalogAsync();
        await SeedBogoAsync(buyProducts: new() { ProdA }, getProducts: new() { ProdA, ProdB }, buyN: 3, getN: 1);
        var sut = BuildSut();

        var result = await sut.ResolveForSaleAsync(
            new List<PricingLine> { Line(VarA, ProdA, qty: 3m, price: 40m) },
            null, DateTime.UtcNow);

        result.BonusItems.Should().HaveCount(2);
        result.BonusItems.Select(b => b.ProductVariantId).Should().BeEquivalentTo(new[] { VarA, VarB });
    }

    [Fact]
    public async Task Partial_discount_percent_is_carried_on_the_bonus()
    {
        await SeedCatalogAsync();
        await SeedBogoAsync(buyProducts: new() { ProdA }, getProducts: new() { ProdB }, buyN: 3, getN: 1, pct: 50m);
        var sut = BuildSut();

        var result = await sut.ResolveForSaleAsync(
            new List<PricingLine> { Line(VarA, ProdA, qty: 3m, price: 40m) },
            null, DateTime.UtcNow);

        result.BonusItems.Single().DiscountPercent.Should().Be(50m);
    }

    [Fact]
    public async Task Reward_quantity_is_capped_at_max_reward_quantity()
    {
        // Buy 1 get 1 free with 10 in cart would earn 10 free, but the cap is 3.
        await SeedCatalogAsync();
        await SeedBogoAsync(buyProducts: new() { ProdA }, getProducts: new() { ProdB }, buyN: 1, getN: 1, maxReward: 3m);
        var sut = BuildSut();

        var result = await sut.ResolveForSaleAsync(
            new List<PricingLine> { Line(VarA, ProdA, qty: 10m, price: 40m) },
            null, DateTime.UtcNow);

        result.BonusItems.Single().Quantity.Should().Be(3m);
    }

    [Fact]
    public async Task Reward_below_cap_is_unaffected()
    {
        await SeedCatalogAsync();
        await SeedBogoAsync(buyProducts: new() { ProdA }, getProducts: new() { ProdB }, buyN: 1, getN: 1, maxReward: 5m);
        var sut = BuildSut();

        var result = await sut.ResolveForSaleAsync(
            new List<PricingLine> { Line(VarA, ProdA, qty: 3m, price: 40m) },
            null, DateTime.UtcNow);

        result.BonusItems.Single().Quantity.Should().Be(3m);
    }

    [Fact]
    public async Task Reward_product_with_no_variant_is_skipped()
    {
        await SeedCatalogAsync();
        await SeedBogoAsync(buyProducts: new() { ProdA }, getProducts: new() { 9999 }, buyN: 3, getN: 1);
        var sut = BuildSut();

        var result = await sut.ResolveForSaleAsync(
            new List<PricingLine> { Line(VarA, ProdA, qty: 3m, price: 40m) },
            null, DateTime.UtcNow);

        result.BonusItems.Should().BeEmpty();
    }
}
