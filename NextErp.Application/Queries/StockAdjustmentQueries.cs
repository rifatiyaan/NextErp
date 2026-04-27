using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Queries;

public record GetStockAdjustmentHistoryQuery(
    int? ProductVariantId,
    int PageIndex = 1,
    int PageSize = 20
) : IRequest<Stock.Response.PagedAdjustments>;
