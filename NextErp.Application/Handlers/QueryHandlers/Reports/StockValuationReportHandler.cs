using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Reports;
using ReportDto = NextErp.Application.DTOs.Report;

namespace NextErp.Application.Handlers.QueryHandlers.Reports;

/// <summary>
/// Joins Stock × ProductVariant × Product to compute inventory value.
/// Cost lives on the Product (added in Phase A); for products without a
/// cost set, the line still appears with Cost=0 so warehouse staff can
/// see which SKUs need backfill.
/// </summary>
public sealed class StockValuationReportHandler(IApplicationDbContext db)
    : IRequestHandler<StockValuationReportQuery, ReportDto.Response.StockValuation>
{
    public async Task<ReportDto.Response.StockValuation> Handle(
        StockValuationReportQuery request,
        CancellationToken cancellationToken = default)
    {
        // Pull active stock with the joined product/category info. We
        // group by product so a multi-variant SKU doesn't double-count
        // the same parent product.
        var rows = await db.Stocks
            .AsNoTracking()
            .Where(s => s.IsActive)
            .Include(s => s.ProductVariant)
                .ThenInclude(pv => pv.Product)
                    .ThenInclude(p => p.Category)
            .Select(s => new
            {
                ProductId = s.ProductVariant!.Product!.Id,
                ProductTitle = s.ProductVariant.Product.Title,
                VariantSku = s.ProductVariant.Sku,
                Category = s.ProductVariant.Product.Category != null
                    ? s.ProductVariant.Product.Category.Title
                    : null,
                Quantity = s.AvailableQuantity,
                UnitCost = s.ProductVariant.Product.Cost,
            })
            .ToListAsync(cancellationToken);

        var lines = rows
            .Select(r => new ReportDto.Response.StockValuation.Line
            {
                ProductId = r.ProductId,
                ProductTitle = r.ProductTitle,
                VariantSku = r.VariantSku,
                Category = r.Category,
                Quantity = r.Quantity,
                UnitCost = r.UnitCost,
                Value = r.Quantity * r.UnitCost,
            })
            .OrderByDescending(l => l.Value)
            .ThenBy(l => l.ProductTitle)
            .ToList();

        return new ReportDto.Response.StockValuation
        {
            AsOf = request.AsOf,
            ProductCount = lines.Count,
            TotalQuantity = lines.Sum(l => l.Quantity),
            TotalValue = lines.Sum(l => l.Value),
            Lines = lines,
        };
    }
}
