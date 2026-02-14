using NextErp.Application.Queries;
using NextErp.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Application.Handlers.QueryHandlers.Stock
{
    public class GetCurrentStockReportHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetCurrentStockReportQuery, DTOs.Stock.Response.CurrentStockReport>
    {
        public async Task<DTOs.Stock.Response.CurrentStockReport> Handle(
            GetCurrentStockReportQuery request,
            CancellationToken cancellationToken)
        {
            var stockDtos = await unitOfWork.StockRepository.Query()
                .AsNoTracking()
                .Include(s => s.Product)
                    .ThenInclude(p => p.Category)
                .Select(s => new DTOs.Stock.Response.Single
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductTitle = s.Product != null ? s.Product.Title : "Unknown",
                    ProductCode = s.Product != null ? s.Product.Code : "N/A",
                    AvailableQuantity = s.AvailableQuantity,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    TenantId = s.TenantId,
                    BranchId = s.BranchId
                })
                .ToListAsync(cancellationToken);

            return new DTOs.Stock.Response.CurrentStockReport
            {
                Stocks = stockDtos,
                TotalProducts = stockDtos.Count,
                TotalQuantity = stockDtos.Sum(s => s.AvailableQuantity)
            };
        }
    }
}
