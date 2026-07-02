namespace NextErp.Application.DTOs.Purchase;

public sealed record PurchaseReportResponse
{
    public List<PurchaseResponse> Purchases { get; init; } = new();
    public decimal TotalPurchaseAmount { get; init; }
    public int TotalPurchases { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
