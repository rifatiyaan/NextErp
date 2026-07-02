using MediatR;
using NextErp.Application.DTOs.Stock;

namespace NextErp.Application.Queries;

public record GetStockAdjustmentHistoryQuery(
    int? ProductVariantId,
    int PageIndex = 1,
    int PageSize = 20
) : IRequest<PagedAdjustments>;
