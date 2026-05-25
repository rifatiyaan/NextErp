using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Dashboard;
using NextErp.Domain.Entities;
using DashboardDto = NextErp.Application.DTOs.Dashboard;

namespace NextErp.Application.Handlers.QueryHandlers.Dashboard;

/// <summary>
/// Builds the homepage dashboard aggregate. Each section is computed in a
/// separate EF query — combining them would force EF to materialise everything
/// upfront, defeating the streaming benefits, and would push us deeper into
/// SQL Server's parameter limit. The handler runs sequentially because EF's
/// DbContext is not thread-safe; pages render fast enough at our data sizes
/// that we don't yet need to push this onto Hangfire or cache it.
/// </summary>
public sealed class GetDashboardOverviewHandler(IApplicationDbContext db)
    : IRequestHandler<GetDashboardOverviewQuery, DashboardDto.Response.Overview>
{
    public async Task<DashboardDto.Response.Overview> Handle(
        GetDashboardOverviewQuery request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = monthStart.AddMonths(-1);

        // ---- Totals block --------------------------------------------------
        // We sum FinalAmount (the post-discount, post-tax bill) because that's
        // what the customer paid and what the rest of the UI displays.
        var salesAll = db.Sales.AsNoTracking().Where(s => s.IsActive);
        var totalRevenue = await salesAll
            .Select(s => (decimal?)s.FinalAmount)
            .SumAsync(cancellationToken) ?? 0m;
        var totalOrders = await salesAll.CountAsync(cancellationToken);
        var totalCustomers = await salesAll
            .Where(s => s.PartyId != null)
            .Select(s => s.PartyId)
            .Distinct()
            .CountAsync(cancellationToken);

        var revenueToday = await salesAll
            .Where(s => s.SaleDate >= todayStart)
            .Select(s => (decimal?)s.FinalAmount)
            .SumAsync(cancellationToken) ?? 0m;
        var ordersToday = await salesAll
            .Where(s => s.SaleDate >= todayStart)
            .CountAsync(cancellationToken);

        var thisMonthRevenue = await salesAll
            .Where(s => s.SaleDate >= monthStart)
            .Select(s => (decimal?)s.FinalAmount)
            .SumAsync(cancellationToken) ?? 0m;
        var lastMonthRevenue = await salesAll
            .Where(s => s.SaleDate >= lastMonthStart && s.SaleDate < monthStart)
            .Select(s => (decimal?)s.FinalAmount)
            .SumAsync(cancellationToken) ?? 0m;

        // Growth% — if last month had zero revenue we treat any positive
        // current revenue as 100% growth (charts hate dividing by zero).
        decimal growth = lastMonthRevenue == 0
            ? (thisMonthRevenue > 0 ? 100m : 0m)
            : decimal.Round(((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100m, 1);

        var activeProductCount = await db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .CountAsync(cancellationToken);

        var lowStockCount = await db.Stocks
            .AsNoTracking()
            .Where(s => s.IsActive && s.ReorderLevel != null && s.AvailableQuantity < s.ReorderLevel)
            .CountAsync(cancellationToken);

        var totals = new DashboardDto.Response.Overview.Totals
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalCustomers = totalCustomers,
            GrowthPercent = growth,
            OrdersToday = ordersToday,
            RevenueToday = revenueToday,
            LowStockCount = lowStockCount,
            ActiveProductCount = activeProductCount,
        };

        // ---- Revenue trend -------------------------------------------------
        // Build the N-month rolling window in C# (not LINQ) so we can use
        // DateTime.AddMonths reliably across DB providers. We pre-bucket the
        // raw rows into a dictionary keyed by year-month string.
        var months = Math.Clamp(request.RevenueTrendMonths, 1, 36);
        var trendStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(months - 1));

        var rawTrend = await salesAll
            .Where(s => s.SaleDate >= trendStart)
            .Select(s => new { s.SaleDate, s.FinalAmount })
            .ToListAsync(cancellationToken);

        var trendByYm = rawTrend
            .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
            .ToDictionary(
                g => $"{g.Key.Year:0000}-{g.Key.Month:00}",
                g => new { Revenue = g.Sum(x => x.FinalAmount), Orders = g.Count() });

        var revenueTrend = new List<DashboardDto.Response.Overview.RevenuePoint>(months);
        for (int i = 0; i < months; i++)
        {
            var d = trendStart.AddMonths(i);
            var key = $"{d.Year:0000}-{d.Month:00}";
            trendByYm.TryGetValue(key, out var bucket);
            revenueTrend.Add(new DashboardDto.Response.Overview.RevenuePoint
            {
                Month = d.ToString("MMM"),
                YearMonth = key,
                Revenue = bucket?.Revenue ?? 0m,
                Orders = bucket?.Orders ?? 0,
            });
        }

        // ---- Top products by revenue --------------------------------------
        var topProductsLimit = Math.Clamp(request.TopProductsLimit, 1, 50);
        var topProducts = await db.SaleItems
            .AsNoTracking()
            .Where(i => i.Sale.IsActive)
            .GroupBy(i => new
            {
                ProductId = i.ProductVariant.Product!.Id,
                Title = i.ProductVariant.Product.Title,
                Sku = i.ProductVariant.Sku,
            })
            .Select(g => new DashboardDto.Response.Overview.ProductRow
            {
                ProductId = g.Key.ProductId,
                Title = g.Key.Title,
                Sku = g.Key.Sku,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * x.Price),
            })
            .OrderByDescending(p => p.Revenue)
            .ThenByDescending(p => p.QuantitySold)
            .Take(topProductsLimit)
            .ToListAsync(cancellationToken);

        // ---- Top customers by total spent ---------------------------------
        var topCustomersLimit = Math.Clamp(request.TopCustomersLimit, 1, 50);
        var topCustomers = await salesAll
            .Where(s => s.PartyId != null && s.Party!.PartyType == PartyType.Customer)
            .GroupBy(s => new { Id = s.PartyId!.Value, Name = s.Party!.Title })
            .Select(g => new DashboardDto.Response.Overview.CustomerRow
            {
                CustomerId = g.Key.Id,
                Name = g.Key.Name,
                OrderCount = g.Count(),
                TotalSpent = g.Sum(x => x.FinalAmount),
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(topCustomersLimit)
            .ToListAsync(cancellationToken);

        // ---- Recent transactions ------------------------------------------
        var recentLimit = Math.Clamp(request.RecentTransactionsLimit, 1, 50);
        var recentTransactions = await salesAll
            .OrderByDescending(s => s.SaleDate)
            .ThenByDescending(s => s.CreatedAt)
            .Take(recentLimit)
            .Select(s => new DashboardDto.Response.Overview.TransactionRow
            {
                SaleId = s.Id,
                SaleNumber = s.SaleNumber,
                CustomerName = s.Party != null ? s.Party.Title : "Walk-in",
                SaleDate = s.SaleDate,
                Amount = s.FinalAmount,
            })
            .ToListAsync(cancellationToken);

        // ---- Sales by category --------------------------------------------
        var salesByCategory = await db.SaleItems
            .AsNoTracking()
            .Where(i => i.Sale.IsActive)
            .GroupBy(i => new
            {
                CategoryId = (int?)i.ProductVariant.Product!.CategoryId,
                CategoryName = i.ProductVariant.Product.Category != null
                    ? i.ProductVariant.Product.Category.Title
                    : "Uncategorised",
            })
            .Select(g => new DashboardDto.Response.Overview.CategorySlice
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                Revenue = g.Sum(x => x.Quantity * x.Price),
                ItemCount = g.Count(),
            })
            .OrderByDescending(c => c.Revenue)
            .ToListAsync(cancellationToken);

        // ---- Activity feed ------------------------------------------------
        // We merge the most-recent sale and purchase rows in memory because
        // EF can't union two different shapes server-side without a heavy
        // mapping layer, and the row counts are tiny (≤ activityLimit each).
        var activityLimit = Math.Clamp(request.ActivityLimit, 1, 50);
        var recentSales = await db.Sales
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .Take(activityLimit)
            .Select(s => new DashboardDto.Response.Overview.ActivityRow
            {
                Kind = "sale",
                Title = $"Sale {s.SaleNumber}",
                Subtitle = s.Party != null ? s.Party.Title : "Walk-in",
                Amount = s.FinalAmount,
                OccurredAt = s.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var recentPurchases = await db.Purchases
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(activityLimit)
            .Select(p => new DashboardDto.Response.Overview.ActivityRow
            {
                Kind = "purchase",
                Title = $"Purchase {p.PurchaseNumber}",
                Subtitle = p.Party != null ? p.Party.Title : "Unknown supplier",
                Amount = p.TotalAmount - p.Discount,
                OccurredAt = p.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var activityFeed = recentSales
            .Concat(recentPurchases)
            .OrderByDescending(a => a.OccurredAt)
            .Take(activityLimit)
            .ToList();

        return new DashboardDto.Response.Overview
        {
            AsOf = now,
            TotalsBlock = totals,
            RevenueTrend = revenueTrend,
            TopProducts = topProducts,
            TopCustomers = topCustomers,
            RecentTransactions = recentTransactions,
            SalesByCategory = salesByCategory,
            ActivityFeed = activityFeed,
        };
    }
}
