namespace NextErp.Application.DTOs.Dashboard;

public sealed record DashboardTotalsResponse
{
    /// <summary>Sum of FinalAmount for all sales ever.</summary>
    public decimal TotalRevenue { get; init; }

    /// <summary>Total number of sale documents ever recorded.</summary>
    public int TotalOrders { get; init; }

    /// <summary>Distinct customers that have at least one sale.</summary>
    public int TotalCustomers { get; init; }

    /// <summary>Revenue this month vs. last month, as a percent (+12.5 means +12.5%).</summary>
    public decimal GrowthPercent { get; init; }

    /// <summary>Sales count for today (00:00 UTC → now).</summary>
    public int OrdersToday { get; init; }

    /// <summary>Sales revenue for today.</summary>
    public decimal RevenueToday { get; init; }

    /// <summary>Number of low-stock products (Available &lt; ReorderLevel).</summary>
    public int LowStockCount { get; init; }

    /// <summary>Number of active products in the catalogue.</summary>
    public int ActiveProductCount { get; init; }
}
