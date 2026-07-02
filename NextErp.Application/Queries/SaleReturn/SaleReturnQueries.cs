using MediatR;
using NextErp.Application.DTOs.Returns;

namespace NextErp.Application.Queries.SaleReturn;

public record GetSaleReturnByIdQuery(Guid Id)
    : IRequest<SaleReturnResponse?>;

public record GetPagedSaleReturnsQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? SearchText = null)
    : IRequest<PagedSaleReturnResponse>;
