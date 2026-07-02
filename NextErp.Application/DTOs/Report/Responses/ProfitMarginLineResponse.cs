namespace NextErp.Application.DTOs.Report;

/// <summary>
/// One line per sale. We aggregate per-sale rather than per-line-item
/// so the report stays a manageable size at high transaction volumes;
/// drill-down into individual items is a separate report.
/// </summary>
public sealed record ProfitMarginLineResponse
{
    public Guid SaleId { get; init; }
    public string SaleNumber { get; init; } = null!;
    public string CustomerName { get; init; } = null!;
    public DateTime SaleDate { get; init; }
    public decimal Revenue { get; init; }
    public decimal Cost { get; init; }
    public decimal Profit { get; init; }
    public decimal MarginPercent { get; init; }
}
