namespace NextErp.Application.DTOs.Returns;

public sealed record CreatePurchaseReturnRequest
{
    public Guid PurchaseId { get; init; }
    public DateTime? ReturnDate { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
    public List<PurchaseReturnLineRequest> Items { get; init; } = new();
}
