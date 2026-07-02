using MediatR;
using NextErp.Application.DTOs.Returns;

namespace NextErp.Application.Queries.PurchaseReturn;

public record GetPurchaseReturnByIdQuery(Guid Id)
    : IRequest<PurchaseReturnResponse?>;

public record GetPagedPurchaseReturnsQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? SearchText = null)
    : IRequest<PagedPurchaseReturnResponse>;
