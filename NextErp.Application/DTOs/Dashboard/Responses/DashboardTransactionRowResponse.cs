namespace NextErp.Application.DTOs.Dashboard;

public sealed record DashboardTransactionRowResponse
{
    public Guid SaleId { get; init; }
    public string SaleNumber { get; init; } = null!;
    public string CustomerName { get; init; } = null!;
    public DateTime SaleDate { get; init; }
    public decimal Amount { get; init; }
}
