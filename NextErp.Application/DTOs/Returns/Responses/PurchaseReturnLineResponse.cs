namespace NextErp.Application.DTOs.Returns;

public sealed record PurchaseReturnLineResponse
{
    public Guid Id { get; init; }
    public Guid PurchaseItemId { get; init; }
    public int ProductVariantId { get; init; }
    public string ProductTitle { get; init; } = null!;
    public string? VariantSku { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Subtotal { get; init; }
    public string? ConditionNote { get; init; }
}
