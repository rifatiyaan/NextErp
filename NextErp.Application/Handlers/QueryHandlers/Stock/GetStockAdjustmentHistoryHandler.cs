using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Common.Extensions;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;
using StockDto = NextErp.Application.DTOs.Stock;

namespace NextErp.Application.Handlers.QueryHandlers.Stock;

public class GetStockAdjustmentHistoryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetStockAdjustmentHistoryQuery, StockDto.Response.PagedAdjustments>
{
    public async Task<StockDto.Response.PagedAdjustments> Handle(
        GetStockAdjustmentHistoryQuery request,
        CancellationToken cancellationToken = default)
    {
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = dbContext.StockMovements
            .AsNoTracking()
            .Where(m => m.MovementType == StockMovementType.ManualAdjustment && m.IsActive)
            .WhereIfHasValue(request.ProductVariantId, m => m.ProductVariantId == request.ProductVariantId!.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(m => m.ProductVariant)
                .ThenInclude(v => v.Product)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new StockDto.Response.AdjustmentLine
            {
                Id = m.Id,
                ProductVariantId = m.ProductVariantId,
                VariantSku = m.ProductVariant.Sku,
                ProductTitle = m.ProductVariant.Product.Title,
                QuantityChanged = m.QuantityChanged,
                PreviousQuantity = m.PreviousQuantity,
                NewQuantity = m.NewQuantity,
                ReasonCode = m.Reason ?? string.Empty,
                Notes = m.Notes,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new StockDto.Response.PagedAdjustments
        {
            Items = items,
            Total = total,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }
}
