using NextErp.Application;
using NextErp.Application.Queries;
using NextErp.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Application.Handlers.QueryHandlers.Stock
{
    public class GetLowStockReportHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetLowStockReportQuery, DTOs.Stock.Response.LowStockReport>
    {
        public async Task<DTOs.Stock.Response.LowStockReport> Handle(
            GetLowStockReportQuery request,
            CancellationToken cancellationToken)
        {
            var lowStockItems = await unitOfWork.StockRepository.Query()
                .AsNoTracking()
                .Include(s => s.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Where(s => s.AvailableQuantity <= 10)
                .Select(s => new DTOs.Stock.Response.LowStockItem
                {
                    ProductVariantId = s.Id,
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
                    ReorderLevel = null,
                    Status = s.AvailableQuantity == 0 ? "Out of Stock"
                        : s.AvailableQuantity <= 5 ? "Critical"
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
