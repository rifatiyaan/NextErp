using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using NextErp.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Application.Handlers.QueryHandlers.Stock
{
    public class GetCurrentStockReportHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetCurrentStockReportQuery, DTOs.Stock.Response.CurrentStockReport>
    {
        public async Task<DTOs.Stock.Response.CurrentStockReport> Handle(
            GetCurrentStockReportQuery request,
            CancellationToken cancellationToken = default)
        {
            var stockDtos = await dbContext.Stocks
                .AsNoTracking()
                .Include(s => s.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                        .ThenInclude(p => p!.UnitOfMeasure)
                .Select(s => new DTOs.Stock.Response.Single
                {
                    Id = s.Id,
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
                    UnitOfMeasureId = s.ProductVariant != null && s.ProductVariant.Product != null
                        ? s.ProductVariant.Product.UnitOfMeasureId : null,
                    UnitOfMeasureAbbreviation = s.ProductVariant != null && s.ProductVariant.Product != null && s.ProductVariant.Product.UnitOfMeasure != null
                        ? s.ProductVariant.Product.UnitOfMeasure.Abbreviation : null,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    TenantId = s.TenantId,
                    BranchId = s.BranchId
                })
                .ToListAsync(cancellationToken);

            return new DTOs.Stock.Response.CurrentStockReport
            {
                Stocks = stockDtos,
                TotalVariants = stockDtos.Count,
                TotalQuantity = stockDtos.Sum(s => s.AvailableQuantity)
            };
        }
    }
}
