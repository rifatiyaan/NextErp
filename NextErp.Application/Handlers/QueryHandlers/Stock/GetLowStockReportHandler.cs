using NextErp.Application.Queries;
using NextErp.Application.DTOs;
using MediatR;

namespace NextErp.Application.Handlers.QueryHandlers.Stock
{
    public class GetLowStockReportHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetLowStockReportQuery, DTOs.Stock.Response.LowStockReport>
    {
        public async Task<DTOs.Stock.Response.LowStockReport> Handle(
            GetLowStockReportQuery request,
            CancellationToken cancellationToken)
        {
            var lowStocks = await unitOfWork.StockRepository.GetLowStockAsync();

            var lowStockItems = lowStocks.Select(s =>
            {
                var status = s.AvailableQuantity == 0 ? "Out of Stock"
                    : s.AvailableQuantity <= 5 ? "Critical"
                    : "Low";

                return new DTOs.Stock.Response.LowStockItem
                {
                    ProductId = s.ProductId,
                    ProductTitle = s.Product?.Title ?? "Unknown",
                    ProductCode = s.Product?.Code ?? "N/A",
                    AvailableQuantity = s.AvailableQuantity,
                    ReorderLevel = null, // TODO: Add ReorderLevel to Product entity if needed
                    Status = status
                };
            }).ToList();

            return new DTOs.Stock.Response.LowStockReport
            {
                Items = lowStockItems,
                TotalLowStockProducts = lowStockItems.Count
            };
        }
    }
}
