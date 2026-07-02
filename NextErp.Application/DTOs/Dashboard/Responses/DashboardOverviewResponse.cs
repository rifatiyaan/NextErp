namespace NextErp.Application.DTOs.Dashboard;

/// <summary>
/// Aggregate dashboard overview surfaced by the Dashboard module. Single shape
/// returned by /api/dashboard/overview — the homepage widgets pull from this
/// one call instead of N parallel requests, so the page renders in one
/// React-Query subscription.
/// </summary>
public sealed record DashboardOverviewResponse
{
    public DateTime AsOf { get; init; }
    public DashboardTotalsResponse TotalsBlock { get; init; } = new();
    public List<DashboardRevenuePointResponse> RevenueTrend { get; init; } = new();
    public List<DashboardProductRowResponse> TopProducts { get; init; } = new();
    public List<DashboardCustomerRowResponse> TopCustomers { get; init; } = new();
    public List<DashboardTransactionRowResponse> RecentTransactions { get; init; } = new();
    public List<DashboardCategorySliceResponse> SalesByCategory { get; init; } = new();
    public List<DashboardActivityRowResponse> ActivityFeed { get; init; } = new();
}
