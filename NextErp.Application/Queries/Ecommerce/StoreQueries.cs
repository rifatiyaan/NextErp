using MediatR;
using NextErp.Application.DTOs.Ecommerce;

namespace NextErp.Application.Queries.Ecommerce
{
    public record GetStoreConfigQuery() : IRequest<StoreConfigResponse>;
    public record GetStoreCategoriesQuery() : IRequest<List<StoreCategoryResponse>>;
    public record GetStorePagedProductsQuery(
        int? CategoryId, string? SearchText, int PageIndex = 1, int PageSize = 24,
        decimal? MinPrice = null, decimal? MaxPrice = null, string? SortBy = null,
        IReadOnlyList<int>? CategoryIds = null)
        : IRequest<StorePagedProductsResponse>;
    public record GetStoreProductByIdQuery(int Id) : IRequest<StoreProductDetailResponse?>;
    public record GetStorePriceRangeQuery(int? CategoryId = null) : IRequest<StorePriceRangeResponse>;
    public record GetProductReviewsQuery(int ProductId) : IRequest<StoreReviewsResponse>;
    public record GetRecentReviewsQuery(int Take) : IRequest<List<StoreRecentReviewRow>>;
    // Public order tracking: returns null unless BOTH the number and phone match.
    public record GetStoreOrderStatusQuery(string OrderNumber, string Phone) : IRequest<StoreOrderStatusResponse?>;

    // Admin: current home hero slides (authorized via the controller).
    public record GetEcommerceHeroSlidesQuery() : IRequest<List<StoreHeroSlide>>;

    // Admin: current homepage section layout JSON (authorized via the controller).
    public record GetHomeLayoutQuery() : IRequest<string>;
}
