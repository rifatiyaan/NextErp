namespace NextErp.Application.DTOs.Returns;

public sealed record PurchaseReturnListRowResponse
{
    public Guid Id { get; init; }
    public string ReturnNumber { get; init; } = null!;
    public Guid PurchaseId { get; init; }
    public string PurchaseNumber { get; init; } = null!;
    public string? SupplierName { get; init; }
    public DateTime ReturnDate { get; init; }
    public decimal TotalAmount { get; init; }
    public int ItemCount { get; init; }
}
