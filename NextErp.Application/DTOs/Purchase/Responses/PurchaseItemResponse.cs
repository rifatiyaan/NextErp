namespace NextErp.Application.DTOs.Purchase;

public sealed record PurchaseItemResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public int ProductVariantId { get; init; }
    public string ProductTitle { get; init; } = null!;
    public string VariantSku { get; init; } = null!;
    public string VariantTitle { get; init; } = null!;
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public decimal Discount { get; init; }
    public string? DiscountSource { get; init; }
    public decimal Total { get; init; }
    public PurchaseItemMetadataResponse? Metadata { get; init; }
}
