namespace NextErp.Application.DTOs.Sale;

public sealed record SaleItemResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public int ProductVariantId { get; init; }
    public string ProductTitle { get; init; } = null!;
    public string VariantSku { get; init; } = null!;
    public string VariantTitle { get; init; } = null!;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Discount { get; init; }
    public string? DiscountSource { get; init; }
    public Guid? PromotionId { get; init; }
    public decimal Total { get; init; }
}
