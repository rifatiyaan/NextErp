using NextErp.Application.Interfaces;
using NextErp.Application.Services;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Services;

public class PricingServiceBogoCrossTests : HandlerTestBase
{
    private const int BuyProductId = 100;       // Pepsi (BUY-set product)
    private const int GetProductId = 200;       // Chips (GET-set product)
    private const int BuyVariantId = 1001;
    private const int GetVariantCheap = 2001;   // Chips small — $1
    private const int GetVariantPricey = 2002;  // Chips large — $3
    private const int OtherCategoryId = 9;      // unrelated

    private PricingService BuildSut() => new(Db);

    private async Task<Guid> SeedBogoCrossAsync(
        int buyN = 2,
        int getN = 1,
        decimal pct = 100m,
        bool stackable = true)
    {
        var id = Guid.NewGuid();
        Db.Promotions.Add(new Promotion
        {
            Id = id,
            Name = "Buy 2 Pepsi get 1 Chips free",
            Type = PromotionType.BogoCross,
            Config = new PromotionConfig
            {
                BuyProductId = BuyProductId,
                GetProductId = GetProductId,
                BuyQuantity = buyN,
                GetQuantity = getN,
                GetDiscountPercent = pct,
            },
            IsActive = true,
            Stackable = stackable,
            TenantId = TenantId,
            CreatedAt = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();
        return id;
    }

    private static PricingLine MakeLine(
        int variantId,
        int productId,
        decimal qty,
        decimal unitPrice,
        int categoryId = 0) =>
        new(variantId, productId, categoryId, qty, unitPrice, ManualDiscount: 0m);

    [Fact]
    public async Task No_BUY_items_in_cart_yields_no_GET_discount()
    {
        await SeedBogoCrossAsync();
        var sut = BuildSut();

        var lines = new List<PricingLine>
        {
            MakeLine(GetVariantCheap, GetProductId, qty: 3m, unitPrice: 1m),
        };
        var result = await sut.ResolveForSaleAsync(lines, null, DateTime.UtcNow);

        result.LineDiscounts.Should().BeEmpty();
    }

    [Fact]
    public async Task Exactly_buyN_BUY_items_funds_one_set_of_GET_discount()
    {
        await SeedBogoCrossAsync(buyN: 2, getN: 1, pct: 100m);
        var sut = BuildSut();

        var lines = new List<PricingLine>
        {
            MakeLine(BuyVariantId, BuyProductId, qty: 2m, unitPrice: 5m),
            MakeLine(GetVariantCheap, GetProductId, qty: 3m, unitPrice: 1m),
        };
        var result = await sut.ResolveForSaleAsync(lines, null, DateTime.UtcNow);

        result.LineDiscounts.Should().ContainSingle();
        var d = result.LineDiscounts.Single();
        d.ProductVariantId.Should().Be(GetVariantCheap);
        d.RuleDiscount.Should().Be(1m); // 1 unit × $1 × 100%
    }

    [Fact]
    public async Task Two_full_buy_sets_funds_two_GET_units()
    {
        await SeedBogoCrossAsync(buyN: 2, getN: 1, pct: 100m);
        var sut = BuildSut();

        var lines = new List<PricingLine>
        {
            MakeLine(BuyVariantId, BuyProductId, qty: 4m, unitPrice: 5m),
            MakeLine(GetVariantCheap, GetProductId, qty: 5m, unitPrice: 1m),
        };
        var result = await sut.ResolveForSaleAsync(lines, null, DateTime.UtcNow);

        result.LineDiscounts.Single().RuleDiscount.Should().Be(2m); // 2 free
    }

    [Fact]
    public async Task Partial_buy_set_floors_to_lower_whole_set()
    {
        await SeedBogoCrossAsync(buyN: 2, getN: 1, pct: 100m);
        var sut = BuildSut();

        var lines = new List<PricingLine>
        {
            MakeLine(BuyVariantId, BuyProductId, qty: 3m, unitPrice: 5m), // floor(3/2) = 1 set
            MakeLine(GetVariantCheap, GetProductId, qty: 5m, unitPrice: 1m),
        };
        var result = await sut.ResolveForSaleAsync(lines, null, DateTime.UtcNow);

        result.LineDiscounts.Single().RuleDiscount.Should().Be(1m);
    }

    [Fact]
    public async Task Cheapest_GET_variant_consumed_first_when_multiple_match()
    {
        // 2 BUY → 1 set → 1 GET unit discounted. Cart has 1 cheap chip
        // ($1) and 1 pricey chip ($3); the cheap one should be the one
        // discounted (store-friendly default).
        await SeedBogoCrossAsync(buyN: 2, getN: 1, pct: 100m);
        var sut = BuildSut();

        var lines = new List<PricingLine>
        {
            MakeLine(BuyVariantId, BuyProductId, qty: 2m, unitPrice: 5m),
            MakeLine(GetVariantPricey, GetProductId, qty: 1m, unitPrice: 3m),
            MakeLine(GetVariantCheap, GetProductId, qty: 1m, unitPrice: 1m),
        };
        var result = await sut.ResolveForSaleAsync(lines, null, DateTime.UtcNow);

        result.LineDiscounts.Should().ContainSingle();
        result.LineDiscounts.Single().ProductVariantId.Should().Be(GetVariantCheap);
    }

    [Fact]
    public async Task Cheapest_first_walks_to_next_GET_line_when_first_exhausted()
    {
        // 4 BUY → 2 sets → 2 GET units. Cheap line only has 1 qty; rest
        // must spill onto the pricey line.
        await SeedBogoCrossAsync(buyN: 2, getN: 1, pct: 100m);
        var sut = BuildSut();

        var lines = new List<PricingLine>
        {
            MakeLine(BuyVariantId, BuyProductId, qty: 4m, unitPrice: 5m),
            MakeLine(GetVariantCheap, GetProductId, qty: 1m, unitPrice: 1m),
            MakeLine(GetVariantPricey, GetProductId, qty: 5m, unitPrice: 3m),
        };
        var result = await sut.ResolveForSaleAsync(lines, null, DateTime.UtcNow);

        result.LineDiscounts.Should().HaveCount(2);
        var cheap = result.LineDiscounts.First(d => d.ProductVariantId == GetVariantCheap);
        var pricey = result.LineDiscounts.First(d => d.ProductVariantId == GetVariantPricey);
        cheap.RuleDiscount.Should().Be(1m);    // 1 × $1 × 100%
        pricey.RuleDiscount.Should().Be(3m);   // 1 × $3 × 100%
    }

    [Fact]
    public async Task GET_units_capped_at_actual_quantity_in_cart()
    {
        // 10 BUY → 5 sets → 5 free chips fundable; cart only has 2 chips,
        // so just 2 get free. Extra fundable units are lost.
        await SeedBogoCrossAsync(buyN: 2, getN: 1, pct: 100m);
        var sut = BuildSut();

        var lines = new List<PricingLine>
        {
            MakeLine(BuyVariantId, BuyProductId, qty: 10m, unitPrice: 5m),
            MakeLine(GetVariantCheap, GetProductId, qty: 2m, unitPrice: 1m),
        };
        var result = await sut.ResolveForSaleAsync(lines, null, DateTime.UtcNow);

        result.LineDiscounts.Single().RuleDiscount.Should().Be(2m);
    }

    [Fact]
    public async Task Half_off_pct_returns_partial_discount_not_free()
    {
        await SeedBogoCrossAsync(buyN: 2, getN: 1, pct: 50m);
        var sut = BuildSut();

        var lines = new List<PricingLine>
        {
            MakeLine(BuyVariantId, BuyProductId, qty: 2m, unitPrice: 5m),
            MakeLine(GetVariantCheap, GetProductId, qty: 1m, unitPrice: 1m),
        };
        var result = await sut.ResolveForSaleAsync(lines, null, DateTime.UtcNow);

        result.LineDiscounts.Single().RuleDiscount.Should().Be(0.5m);
    }
}
