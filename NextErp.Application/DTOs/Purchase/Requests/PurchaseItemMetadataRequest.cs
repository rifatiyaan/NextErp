namespace NextErp.Application.DTOs.Purchase;

public sealed record PurchaseItemMetadataRequest
{
    public string? Description { get; init; }
    public decimal? Weight { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? BatchNumber { get; init; }
}
