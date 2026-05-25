using NextErp.Application.Handlers.QueryHandlers.Sale;
using NextErp.Application.Queries;
using NextErp.Application.Services;
using NextErp.Application.Tests.Builders;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;
using SaleDto = NextErp.Application.DTOs.Sale;

namespace NextErp.Application.Tests.Handlers.Sale;

public class PreviewSalePricingHandlerTests : HandlerTestBase
{
    private const int VariantA = 101;
    private const int VariantB = 102;
    private const int ProductId = 1;
    private const int CategoryId = 1;

    private PreviewSalePricingHandler BuildHandler() =>
        new(Db, new PricingService(Db));

    private async Task SeedCatalogAsync(decimal priceA = 100m, decimal priceB = 50m)
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        Db.Categories.Add(new CategoryBuilder().WithId(CategoryId).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Products.Add(new ProductBuilder()
            .WithId(ProductId).WithCategory(CategoryId).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantA).WithProduct(ProductId).WithSku("A").WithPrice(priceA)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.ProductVariants.Add(new ProductVariantBuilder()
            .WithId(VariantB).WithProduct(ProductId).WithSku("B").WithPrice(priceB)
            .WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
    }

    private async Task<Guid> SeedLineDiscountPromotionAsync(string name, decimal discountAmount)
    {
        var id = Guid.NewGuid();
        Db.Promotions.Add(new Promotion
        {
            Id = id,
            Name = name,
            Type = PromotionType.LineDiscount,
            Config = new PromotionConfig
            {
                DiscountAmount = discountAmount,
                ScopeProductIds = new List<int> { ProductId },
            },
            IsActive = true,
            Stackable = true,
            TenantId = TenantId,
            CreatedAt = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Empty_cart_returns_empty_result_with_zero_totals()
    {
        var sut = BuildHandler();

        var result = await sut.Handle(
            new PreviewSalePricingQuery(new List<SaleDto.Request.Preview.PreviewLineRequest>(), null),
            CancellationToken.None);

        result.Subtotal.Should().Be(0m);
        result.FinalAmount.Should().Be(0m);
        result.LineDiscounts.Should().BeEmpty();
        result.InvoiceDiscount.Should().Be(0m);
        result.InvoicePromotionId.Should().BeNull();
        result.InvoicePromotionName.Should().BeNull();
    }

    [Fact]
    public async Task Unknown_variant_id_is_skipped_not_thrown()
    {
        await SeedCatalogAsync();
        var sut = BuildHandler();

        var result = await sut.Handle(
            new PreviewSalePricingQuery(new List<SaleDto.Request.Preview.PreviewLineRequest>
            {
                new() { ProductVariantId = 9999, Quantity = 1m, UnitPrice = 100m },
                new() { ProductVariantId = VariantA, Quantity = 2m, UnitPrice = 100m },
            }, null),
            CancellationToken.None);

        // Only the known variant contributes to subtotal — unknown one is silently dropped.
        result.Subtotal.Should().Be(200m);
        result.FinalAmount.Should().Be(200m);
    }

    [Fact]
    public async Task Line_discount_promotion_returns_discount_with_promotion_name()
    {
        await SeedCatalogAsync(priceA: 100m);
        var promoId = await SeedLineDiscountPromotionAsync("$10 off catalog", 10m);
        var sut = BuildHandler();

        var result = await sut.Handle(
            new PreviewSalePricingQuery(new List<SaleDto.Request.Preview.PreviewLineRequest>
            {
                new() { ProductVariantId = VariantA, Quantity = 2m, UnitPrice = 100m },
            }, null),
            CancellationToken.None);

        result.LineDiscounts.Should().HaveCount(1);
        var line = result.LineDiscounts[0];
        line.ProductVariantId.Should().Be(VariantA);
        line.Discount.Should().Be(10m);
        line.PromotionId.Should().Be(promoId);
        line.PromotionName.Should().Be("$10 off catalog");

        // 200 gross − 10 rule discount = 190 subtotal; no invoice-level → final = 190.
        result.Subtotal.Should().Be(190m);
        result.FinalAmount.Should().Be(190m);
    }

    [Fact]
    public async Task Manual_line_discount_is_subtracted_from_subtotal()
    {
        await SeedCatalogAsync(priceA: 100m);
        var sut = BuildHandler();

        var result = await sut.Handle(
            new PreviewSalePricingQuery(new List<SaleDto.Request.Preview.PreviewLineRequest>
            {
                new() { ProductVariantId = VariantA, Quantity = 2m, UnitPrice = 100m, ManualDiscount = 25m },
            }, null),
            CancellationToken.None);

        // 200 gross − 25 manual = 175.
        result.Subtotal.Should().Be(175m);
        result.FinalAmount.Should().Be(175m);
        result.LineDiscounts.Should().BeEmpty();
    }

    [Fact]
    public async Task Null_party_id_skips_membership_promotion()
    {
        await SeedCatalogAsync(priceA: 100m);
        Db.Promotions.Add(new Promotion
        {
            Id = Guid.NewGuid(),
            Name = "Gold tier 10% off",
            Type = PromotionType.Membership,
            Config = new PromotionConfig
            {
                DiscountPercent = 10m,
                MembershipTier = "Gold",
            },
            IsActive = true,
            Stackable = true,
            TenantId = TenantId,
            CreatedAt = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();
        var sut = BuildHandler();

        var result = await sut.Handle(
            new PreviewSalePricingQuery(new List<SaleDto.Request.Preview.PreviewLineRequest>
            {
                new() { ProductVariantId = VariantA, Quantity = 1m, UnitPrice = 100m },
            }, PartyId: null),
            CancellationToken.None);

        // PricingService skips Membership lookup entirely when partyId is null.
        result.InvoiceDiscount.Should().Be(0m);
        result.InvoicePromotionId.Should().BeNull();
        result.FinalAmount.Should().Be(100m);
    }

    [Fact]
    public async Task Preview_does_not_persist_any_sale_rows()
    {
        await SeedCatalogAsync();
        await SeedLineDiscountPromotionAsync("Catalog 5%", 5m);
        var sut = BuildHandler();

        await sut.Handle(
            new PreviewSalePricingQuery(new List<SaleDto.Request.Preview.PreviewLineRequest>
            {
                new() { ProductVariantId = VariantA, Quantity = 3m, UnitPrice = 100m },
            }, null),
            CancellationToken.None);

        (await Db.Sales.AnyAsync()).Should().BeFalse();
        (await Db.SaleItems.AnyAsync()).Should().BeFalse();
        (await Db.StockMovements.AnyAsync()).Should().BeFalse();
    }
}
