namespace NextErp.Application.DTOs.Sale;

public sealed record PreviewSaleResponse
{
    public decimal Subtotal { get; init; }
    public List<PreviewLineDiscountResponse> LineDiscounts { get; init; } = new();
    public List<PreviewBonusItemResponse> BonusItems { get; init; } = new();
    public decimal InvoiceDiscount { get; init; }
    public Guid? InvoicePromotionId { get; init; }
    public string? InvoicePromotionName { get; init; }
    public decimal FinalAmount { get; init; }
}
