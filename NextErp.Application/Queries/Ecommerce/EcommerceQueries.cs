using MediatR;
using NextErp.Application.DTOs.Ecommerce;

namespace NextErp.Application.Queries.Ecommerce
{
    public record GetEcommercePublicationQuery() : IRequest<List<PublicationCategoryResponse>>;

    public record GetPagedOnlineOrdersQuery(string? Status, int PageIndex = 1, int PageSize = 20) : IRequest<PagedOnlineOrdersResponse>;
    public record GetOnlineOrderByIdQuery(int Id) : IRequest<OnlineOrderDetailResponse?>;
}
