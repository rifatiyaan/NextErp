namespace NextErp.Application.DTOs.Purchase;

public sealed record CreatePurchaseRequest
{
    public string Title { get; init; } = null!;
    public string PurchaseNumber { get; init; } = null!;
    public Guid? PartyId { get; init; }
    public DateTime PurchaseDate { get; init; }
    public decimal Discount { get; init; }
    public List<PurchaseItemRequest> Items { get; init; } = new();
    public PurchaseMetadataRequest Metadata { get; init; } = new();
}
