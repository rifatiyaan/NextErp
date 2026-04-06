using MediatR;
using NextErp.Application.DTOs;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Queries
{
    public record GetStockByProductVariantIdQuery(int ProductVariantId) : IRequest<Entities.Stock?>;

    public record GetStocksByProductIdQuery(int ProductId) : IRequest<IReadOnlyList<Entities.Stock>>;

    public record GetCurrentStockReportQuery() : IRequest<Stock.Response.CurrentStockReport>;

    public record GetLowStockReportQuery() : IRequest<Stock.Response.LowStockReport>;

    public record GetStockMovementHistoryQuery(int ProductVariantId, Guid BranchId)
        : IRequest<IReadOnlyList<Stock.Response.MovementLine>>;
}
