using MediatR;
using NextErp.Application.DTOs.Dashboard;

namespace NextErp.Application.Queries.Dashboard;

/// <summary>
/// Single overview query that powers the homepage dashboard. We return one
/// fat aggregate instead of fanning out into N small endpoints so the page
/// can subscribe with one React-Query key and a single round-trip — cheaper
/// for a frequently-visited page and easier to invalidate after writes.
/// </summary>
/// <param name="RevenueTrendMonths">Months of history to include in the trend chart (default 12).</param>
/// <param name="TopProductsLimit">How many top products to return (default 5).</param>
/// <param name="TopCustomersLimit">How many top customers to return (default 5).</param>
/// <param name="RecentTransactionsLimit">How many recent sales to return (default 10).</param>
/// <param name="ActivityLimit">How many recent activity items to return (default 10).</param>
public record GetDashboardOverviewQuery(
    int RevenueTrendMonths = 12,
    int TopProductsLimit = 5,
    int TopCustomersLimit = 5,
    int RecentTransactionsLimit = 10,
    int ActivityLimit = 10)
    : IRequest<DashboardOverviewResponse>;
