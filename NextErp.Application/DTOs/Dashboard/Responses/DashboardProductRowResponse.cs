namespace NextErp.Application.DTOs.Dashboard;

public sealed record DashboardProductRowResponse
{
    public int ProductId { get; init; }
    public string Title { get; init; } = null!;
    public string? Sku { get; init; }
    public decimal QuantitySold { get; init; }
    public decimal Revenue { get; init; }
}
