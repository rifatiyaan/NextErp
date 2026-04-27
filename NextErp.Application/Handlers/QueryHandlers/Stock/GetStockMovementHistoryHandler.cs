using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Common.Exceptions;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using StockDto = NextErp.Application.DTOs.Stock;

namespace NextErp.Application.Handlers.QueryHandlers.Stock;

public class GetStockMovementHistoryHandler(
    IApplicationDbContext dbContext,
    IBranchProvider branchProvider)
    : IRequestHandler<GetStockMovementHistoryQuery, IReadOnlyList<StockDto.Response.MovementLine>>
{
    public async Task<IReadOnlyList<StockDto.Response.MovementLine>> Handle(
        GetStockMovementHistoryQuery request,
        CancellationToken cancellationToken = default)
    {
        if (!branchProvider.IsGlobal())
        {
            var userBranch = branchProvider.GetRequiredBranchId();
            if (userBranch != request.BranchId)
                throw new ForbiddenAccessException("You can only view stock movement history for your branch.");
        }

        return await dbContext.StockMovements
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(m =>
                m.ProductVariantId == request.ProductVariantId
                && m.BranchId == request.BranchId
                && m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new StockDto.Response.MovementLine
            {
                Id = m.Id,
                StockId = m.StockId,
                ProductVariantId = m.ProductVariantId,
                BranchId = m.BranchId,
                QuantityChanged = m.QuantityChanged,
                PreviousQuantity = m.PreviousQuantity,
                NewQuantity = m.NewQuantity,
                MovementType = m.MovementType.ToString(),
                ReferenceId = m.ReferenceId,
                Reason = m.Reason,
                Notes = m.Notes,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
