namespace NextErp.Application.DTOs.Returns;

public sealed record PurchaseReturnResponse
{
    public Guid Id { get; init; }
    public string ReturnNumber { get; init; } = null!;
    public Guid PurchaseId { get; init; }
    public string PurchaseNumber { get; init; } = null!;
    public string? SupplierName { get; init; }
    public DateTime ReturnDate { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
    public decimal TotalAmount { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<PurchaseReturnLineResponse> Items { get; init; } = new();
}
