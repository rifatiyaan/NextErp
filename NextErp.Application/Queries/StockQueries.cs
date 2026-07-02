using MediatR;
using NextErp.Application.DTOs.Stock;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Queries
{
    public record GetStockByProductVariantIdQuery(int ProductVariantId) : IRequest<Entities.Stock?>;

    public record GetStocksByProductIdQuery(int ProductId) : IRequest<IReadOnlyList<Entities.Stock>>;

    public record GetCurrentStockReportQuery() : IRequest<CurrentStockReport>;

    public record GetLowStockReportQuery() : IRequest<LowStockReport>;

    public record GetStockMovementHistoryQuery(int ProductVariantId, Guid BranchId)
        : IRequest<IReadOnlyList<MovementLine>>;
}
