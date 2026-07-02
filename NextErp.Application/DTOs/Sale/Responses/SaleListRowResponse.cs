namespace NextErp.Application.DTOs.Sale;

public sealed record SaleListRowResponse
{
    public Guid Id { get; init; }
    public string SaleNumber { get; init; } = null!;
    public string CustomerName { get; init; } = null!;
    public DateTime SaleDate { get; init; }
    public decimal FinalAmount { get; init; }
    public decimal TotalPaid { get; init; }
    public decimal BalanceDue { get; init; }
}
