using MediatR;
using NextErp.Application.DTOs.Ecommerce;

namespace NextErp.Application.Queries.Ecommerce
{
    public record GetStoreConfigQuery() : IRequest<StoreConfigResponse>;
    public record GetStoreCategoriesQuery() : IRequest<List<StoreCategoryResponse>>;
    public record GetStorePagedProductsQuery(
        int? CategoryId, string? SearchText, int PageIndex = 1, int PageSize = 24,
        decimal? MinPrice = null, decimal? MaxPrice = null, string? SortBy = null)
        : IRequest<StorePagedProductsResponse>;
    public record GetStoreProductByIdQuery(int Id) : IRequest<StoreProductDetailResponse?>;
    public record GetStorePriceRangeQuery(int? CategoryId = null) : IRequest<StorePriceRangeResponse>;
    public record GetProductReviewsQuery(int ProductId) : IRequest<StoreReviewsResponse>;
    // Public order tracking: returns null unless BOTH the number and phone match.
    public record GetStoreOrderStatusQuery(string OrderNumber, string Phone) : IRequest<StoreOrderStatusResponse?>;

    // Admin: current home hero slides (authorized via the controller).
    public record GetEcommerceHeroSlidesQuery() : IRequest<List<StoreHeroSlide>>;
}
