namespace NextErp.Application.DTOs.Sale;

public sealed record SaleReportResponse
{
    public List<SaleResponse> Sales { get; init; } = new();
    public decimal TotalSalesAmount { get; init; }
    public int TotalSales { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
