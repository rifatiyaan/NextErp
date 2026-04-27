using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using NextErp.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Application.Handlers.QueryHandlers.Stock
{
    public class GetLowStockReportHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetLowStockReportQuery, DTOs.Stock.Response.LowStockReport>
    {
        public async Task<DTOs.Stock.Response.LowStockReport> Handle(
            GetLowStockReportQuery request,
            CancellationToken cancellationToken = default)
        {
            var lowStockItems = await dbContext.Stocks
                .AsNoTracking()
                .Include(s => s.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                        .ThenInclude(p => p!.UnitOfMeasure)
                .Where(s => s.AvailableQuantity <= (s.ReorderLevel ?? 10))
                .Select(s => new DTOs.Stock.Response.LowStockItem
                {
                    ProductVariantId = s.ProductVariantId,
                    ProductId = s.ProductVariant != null ? s.ProductVariant.ProductId : 0,
                    ProductTitle = s.ProductVariant != null && s.ProductVariant.Product != null
                        ? s.ProductVariant.Product.Title
                        : "Unknown",
                    ProductCode = s.ProductVariant != null && s.ProductVariant.Product != null
                        ? s.ProductVariant.Product.Code
                        : "N/A",
                    VariantSku = s.ProductVariant != null ? s.ProductVariant.Sku : "",
                    VariantTitle = s.ProductVariant != null ? s.ProductVariant.Title : "",
                    AvailableQuantity = s.AvailableQuantity,
                    ReorderLevel = s.ReorderLevel,
                    UnitOfMeasureAbbreviation = s.ProductVariant != null && s.ProductVariant.Product != null && s.ProductVariant.Product.UnitOfMeasure != null
                        ? s.ProductVariant.Product.UnitOfMeasure.Abbreviation : null,
                    Status = s.AvailableQuantity == 0 ? "Out of Stock"
                        : s.AvailableQuantity <= (s.ReorderLevel.HasValue ? s.ReorderLevel.Value * 0.5m : 5m) ? "Critical"
                        : "Low"
                })
                .ToListAsync(cancellationToken);

            return new DTOs.Stock.Response.LowStockReport
            {
                Items = lowStockItems,
                TotalLowStockVariants = lowStockItems.Count
            };
        }
    }
}
