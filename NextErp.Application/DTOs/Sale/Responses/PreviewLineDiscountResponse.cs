namespace NextErp.Application.DTOs.Sale;

public sealed record PreviewLineDiscountResponse
{
    public int ProductVariantId { get; init; }
    public decimal Discount { get; init; }
    public Guid? PromotionId { get; init; }
    public string? PromotionName { get; init; }
}
