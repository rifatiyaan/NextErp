namespace NextErp.Application.DTOs.Purchase;

public sealed record PurchaseResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string PurchaseNumber { get; init; } = null!;
    public Guid? PartyId { get; init; }
    public string SupplierName { get; init; } = null!;
    public DateTime PurchaseDate { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal Discount { get; init; }
    public decimal NetTotal { get; init; }
    public List<PurchaseItemResponse> Items { get; init; } = new();
    public PurchaseMetadataRequest Metadata { get; init; } = new();
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid? BranchId { get; init; }
}
