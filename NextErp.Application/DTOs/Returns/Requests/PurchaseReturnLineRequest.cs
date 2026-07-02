namespace NextErp.Application.DTOs.Returns;

public sealed record PurchaseReturnLineRequest
{
    public Guid PurchaseItemId { get; init; }
    public int ProductVariantId { get; init; }
    public decimal Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public string? ConditionNote { get; init; }
}
