using MediatR;
using NextErp.Application.DTOs.Ecommerce;

namespace NextErp.Application.Queries.Ecommerce
{
    public record GetStoreConfigQuery() : IRequest<StoreConfigResponse>;
    public record GetStoreCategoriesQuery() : IRequest<List<StoreCategoryResponse>>;
    public record GetStorePagedProductsQuery(
        int? CategoryId, string? SearchText, int PageIndex = 1, int PageSize = 24)
        : IRequest<StorePagedProductsResponse>;
    public record GetStoreProductByIdQuery(int Id) : IRequest<StoreProductDetailResponse?>;
}
