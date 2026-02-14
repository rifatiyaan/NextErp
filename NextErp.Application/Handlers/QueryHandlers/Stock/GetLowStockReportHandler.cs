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
                .Include(s => s.Product)
                    .ThenInclude(p => p.Category)
                .Where(s => s.AvailableQuantity <= 10)
                .Select(s => new DTOs.Stock.Response.LowStockItem
                {
                    ProductId = s.ProductId,
                    ProductTitle = s.Product != null ? s.Product.Title : "Unknown",
                    ProductCode = s.Product != null ? s.Product.Code : "N/A",
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
                TotalLowStockProducts = lowStockItems.Count
            };
        }
    }
}
