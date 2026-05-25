using MediatR;
using NextErp.Application.DTOs.Returns;

namespace NextErp.Application.Queries.PurchaseReturn;

public record GetPurchaseReturnByIdQuery(Guid Id)
    : IRequest<PurchaseReturnDto.Response.Get.Single?>;

public record GetPagedPurchaseReturnsQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? SearchText = null)
    : IRequest<PurchaseReturnDto.Response.Paged>;
