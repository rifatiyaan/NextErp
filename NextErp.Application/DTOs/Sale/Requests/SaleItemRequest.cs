namespace NextErp.Application.DTOs.Sale;

public sealed record SaleItemRequest
{
    public int ProductVariantId { get; init; }
    public decimal Quantity { get; init; }
    public decimal Price { get; init; }
    public decimal Subtotal { get; init; }

    /// <summary>
    /// Optional per-line discount typed by the operator. The
    /// handler subtracts this from (Quantity × Price) to get
    /// the line total. Promotion-engine-applied discounts
    /// land in the same field but with DiscountSource=Promotion.
    /// </summary>
    public decimal? Discount { get; init; }
}
