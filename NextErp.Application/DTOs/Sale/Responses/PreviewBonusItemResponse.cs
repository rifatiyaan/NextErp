namespace NextErp.Application.DTOs.Sale;

public sealed record PreviewBonusItemResponse
{
    public int ProductVariantId { get; init; }
    public string Title { get; init; } = null!;
    public string? ImageUrl { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountPercent { get; init; }
    public Guid PromotionId { get; init; }
    public string? PromotionName { get; init; }
}
