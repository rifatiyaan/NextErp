namespace NextErp.Application.DTOs.Purchase;

public sealed record PurchaseItemRequest
{
    public string Title { get; init; } = null!;
    public int ProductVariantId { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }

    /// <summary>
    /// Optional supplier-negotiated per-line discount. Caps at
    /// (Quantity × UnitCost) so the line never goes negative.
    /// Purchase side is manual-only (no rule engine in MVP).
    /// </summary>
    public decimal? Discount { get; init; }

    public PurchaseItemMetadataRequest? Metadata { get; init; }
}
