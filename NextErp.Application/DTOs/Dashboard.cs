namespace NextErp.Application.DTOs;

/// <summary>
/// Aggregate dashboard overview surfaced by the Dashboard module. Single shape
/// returned by /api/dashboard/overview — the homepage widgets pull from this
/// one call instead of N parallel requests, so the page renders in one
/// React-Query subscription.
/// </summary>
public partial class Dashboard
{
    public partial class Response
    {
        public partial class Overview
        {
            public DateTime AsOf { get; set; }
            public Totals TotalsBlock { get; set; } = new();
            public List<RevenuePoint> RevenueTrend { get; set; } = new();
            public List<ProductRow> TopProducts { get; set; } = new();
            public List<CustomerRow> TopCustomers { get; set; } = new();
            public List<TransactionRow> RecentTransactions { get; set; } = new();
            public List<CategorySlice> SalesByCategory { get; set; } = new();
            public List<ActivityRow> ActivityFeed { get; set; } = new();

            public class Totals
            {
                /// <summary>Sum of FinalAmount for all sales ever.</summary>
                public decimal TotalRevenue { get; set; }

                /// <summary>Total number of sale documents ever recorded.</summary>
                public int TotalOrders { get; set; }

                /// <summary>Distinct customers that have at least one sale.</summary>
                public int TotalCustomers { get; set; }

                /// <summary>Revenue this month vs. last month, as a percent (+12.5 means +12.5%).</summary>
                public decimal GrowthPercent { get; set; }

                /// <summary>Sales count for today (00:00 UTC → now).</summary>
                public int OrdersToday { get; set; }

                /// <summary>Sales revenue for today.</summary>
                public decimal RevenueToday { get; set; }

                /// <summary>Number of low-stock products (Available &lt; ReorderLevel).</summary>
                public int LowStockCount { get; set; }

                /// <summary>Number of active products in the catalogue.</summary>
                public int ActiveProductCount { get; set; }
            }

            public class RevenuePoint
            {
                /// <summary>"Jan", "Feb", … (3-letter month label).</summary>
                public string Month { get; set; } = null!;

                /// <summary>YYYY-MM string for unambiguous ordering on the client.</summary>
                public string YearMonth { get; set; } = null!;

                public decimal Revenue { get; set; }
                public int Orders { get; set; }
            }

            public class ProductRow
            {
                public int ProductId { get; set; }
                public string Title { get; set; } = null!;
                public string? Sku { get; set; }
                public decimal QuantitySold { get; set; }
                public decimal Revenue { get; set; }
            }

            public class CustomerRow
            {
                public Guid CustomerId { get; set; }
                public string Name { get; set; } = null!;
                public int OrderCount { get; set; }
                public decimal TotalSpent { get; set; }
            }

            public class TransactionRow
            {
                public Guid SaleId { get; set; }
                public string SaleNumber { get; set; } = null!;
                public string CustomerName { get; set; } = null!;
                public DateTime SaleDate { get; set; }
                public decimal Amount { get; set; }
            }

            public class CategorySlice
            {
                public int? CategoryId { get; set; }
                public string CategoryName { get; set; } = null!;
                public decimal Revenue { get; set; }
                public int ItemCount { get; set; }
            }

            public class ActivityRow
            {
                /// <summary>"sale", "purchase", or "stock" — small enough that a string is fine.</summary>
                public string Kind { get; set; } = null!;
                public string Title { get; set; } = null!;
                public string? Subtitle { get; set; }
                public decimal? Amount { get; set; }
                public DateTime OccurredAt { get; set; }
            }
        }
    }
}
