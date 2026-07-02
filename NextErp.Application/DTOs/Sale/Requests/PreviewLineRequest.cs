namespace NextErp.Application.DTOs.Sale;

public sealed record PreviewLineRequest
{
    public int ProductVariantId { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal ManualDiscount { get; init; }
}
