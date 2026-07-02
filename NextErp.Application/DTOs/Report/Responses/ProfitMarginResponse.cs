namespace NextErp.Application.DTOs.Report;

public sealed record ProfitMarginResponse
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int SaleCount { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal TotalCost { get; init; }
    public decimal TotalProfit { get; init; }
    public decimal AverageMarginPercent { get; init; }
    public List<ProfitMarginLineResponse> Lines { get; init; } = new();
}
