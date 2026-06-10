using NextErp.Domain.Entities;

namespace NextErp.Application.Interfaces;

/// <summary>
/// Resolves which Promotions apply to a sale-in-progress and returns the
/// concrete $ discounts to stamp on each line + the invoice. The handler
/// calls this once with the proposed lines + customer; the returned
/// PricingResolution then drives <c>SaleItem.Discount</c>,
/// <c>SaleItem.PromotionId</c>, and <c>Sale.InvoicePromotionId</c>.
/// </summary>
public interface IPricingService
{
    Task<PricingResolution> ResolveForSaleAsync(
        IReadOnlyList<PricingLine> lines,
        Guid? partyId,
        DateTime asOf,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Input line passed into the pricing engine. Carries the raw qty/price
/// the operator entered + the product/category hints needed for matching
/// rules. Operator-set manual discount (if any) is carried so we can
/// stack it on top of rule output.
/// </summary>
public sealed record PricingLine(
    int ProductVariantId,
    int ProductId,
    int CategoryId,
    decimal Quantity,
    decimal UnitPrice,
    decimal ManualDiscount);

/// <summary>
/// Output of the pricing engine. <see cref="LineDiscounts"/> is indexed
/// by ProductVariantId; <see cref="InvoiceDiscount"/> is the absolute $
/// off the subtotal.
/// </summary>
public sealed class PricingResolution
{
    public IReadOnlyList<LineResolution> LineDiscounts { get; init; } = Array.Empty<LineResolution>();
    // Free units added on top of cart qty (BogoSame "bonus" semantic).
    // CreateSaleHandler turns these into phantom SaleItems at 100% discount.
    public IReadOnlyList<BonusItem> BonusItems { get; init; } = Array.Empty<BonusItem>();
    public decimal InvoiceDiscount { get; init; }
    public Guid? InvoicePromotionId { get; init; }
}

public sealed record LineResolution(
    int ProductVariantId,
    decimal RuleDiscount,
    Guid? PromotionId);

public sealed record BonusItem(
    int ProductVariantId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent,   // 100 = free; <100 = added at a reduced price
    Guid PromotionId);
