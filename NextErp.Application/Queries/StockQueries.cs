using MediatR;
using NextErp.Application.DTOs;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Queries
{
    // Get stock by product Id
    public record GetStockByProductIdQuery(int ProductId) : IRequest<Entities.Stock?>;

    // Get current stock report
    public record GetCurrentStockReportQuery() : IRequest<Stock.Response.CurrentStockReport>;

    // Get low stock report
    public record GetLowStockReportQuery() : IRequest<Stock.Response.LowStockReport>;
}
