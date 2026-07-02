namespace NextErp.Application.DTOs.Purchase;

public sealed record CreatePurchaseResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string PurchaseNumber { get; init; } = null!;
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}
