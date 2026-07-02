using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Reports;
using NextErp.Application.DTOs.Report;

namespace NextErp.Application.Handlers.QueryHandlers.Reports;

/// <summary>
/// Computes revenue – cost per sale across a date range. Cost is sourced
/// from <c>Product.Cost</c> (set Phase A) at the time of the report; if a
/// product has cost=0 the margin shows 100%, which is a useful "fix this
/// data" signal in the report.
/// </summary>
public sealed class ProfitMarginReportHandler(IApplicationDbContext db)
    : IRequestHandler<ProfitMarginReportQuery, ProfitMarginResponse>
{
    public async Task<ProfitMarginResponse> Handle(
        ProfitMarginReportQuery request,
        CancellationToken cancellationToken = default)
    {
        // Walk sales × items × variant × product so we can multiply each
        // line item's quantity by its cost. Done as a single round-trip;
        // the projection keeps the payload tight (no full Sale graph).
        var saleData = await db.Sales
            .AsNoTracking()
            .Where(s => s.SaleDate >= request.StartDate && s.SaleDate <= request.EndDate)
            .Include(s => s.Party)
            .Include(s => s.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(pv => pv!.Product)
            .Select(s => new
            {
                s.Id,
                s.SaleNumber,
                CustomerName = s.Party != null ? s.Party.Title : "Walk-in",
                s.SaleDate,
                Revenue = s.FinalAmount,
                // Sum (quantity * unit cost) across line items. Falls back
                // to 0 for orphaned variants (shouldn't happen but defensive).
                Cost = s.Items.Sum(i =>
                    i.ProductVariant != null && i.ProductVariant.Product != null
                        ? i.Quantity * i.ProductVariant.Product.Cost
                        : 0m),
            })
            .ToListAsync(cancellationToken);

        var lines = saleData
            .Select(s =>
            {
                var profit = s.Revenue - s.Cost;
                var marginPct = s.Revenue > 0 ? profit / s.Revenue * 100m : 0m;
                return new ProfitMarginLineResponse
                {
                    SaleId = s.Id,
                    SaleNumber = s.SaleNumber,
                    CustomerName = s.CustomerName,
                    SaleDate = s.SaleDate,
                    Revenue = s.Revenue,
                    Cost = s.Cost,
                    Profit = profit,
                    MarginPercent = Math.Round(marginPct, 2),
                };
            })
            .OrderByDescending(l => l.SaleDate)
            .ToList();

        var totalRevenue = lines.Sum(l => l.Revenue);
        var totalCost = lines.Sum(l => l.Cost);
        var totalProfit = totalRevenue - totalCost;
        var avgMarginPct = totalRevenue > 0
            ? Math.Round(totalProfit / totalRevenue * 100m, 2)
            : 0m;

        return new ProfitMarginResponse
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            SaleCount = lines.Count,
            TotalRevenue = totalRevenue,
            TotalCost = totalCost,
            TotalProfit = totalProfit,
            AverageMarginPercent = avgMarginPct,
            Lines = lines,
        };
    }
}
