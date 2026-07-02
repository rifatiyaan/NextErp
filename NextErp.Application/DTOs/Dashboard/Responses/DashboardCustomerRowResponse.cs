namespace NextErp.Application.DTOs.Dashboard;

public sealed record DashboardCustomerRowResponse
{
    public Guid CustomerId { get; init; }
    public string Name { get; init; } = null!;
    public int OrderCount { get; init; }
    public decimal TotalSpent { get; init; }
}
