namespace NextErp.Application.DTOs.Ecommerce;

public sealed record StoreConfigResponse(bool StorefrontEnabled, string StoreName, string Tagline, string HeroHeadline, string HeroImageUrl, string MarqueeText, string CodNote, decimal DeliveryFee);
public sealed record StoreCategoryResponse(int Id, string Title, int? ParentId, int ProductCount, string? ImageUrl);
public sealed record StoreProductRow(int Id, string Title, decimal Price, string? ImageUrl, string? SecondImageUrl, bool InStock, decimal? LowStockQuantity, bool HasVariations);
public sealed record StorePagedProductsResponse(int Total, List<StoreProductRow> Data);
public sealed record StorePriceRangeResponse(decimal Min, decimal Max);
public sealed record StoreVariantRow(int Id, string Sku, string Title, decimal Price, bool InStock, decimal? LowStockQuantity);
public sealed record StoreProductDetailResponse(int Id, string Title, decimal Price, string? Description, string? CategoryTitle, int CategoryId, List<string> Images, List<StoreVariantRow> Variants);

public sealed record StoreReviewRow(int Id, string AuthorName, int Rating, string Text, DateTime CreatedAt);
public sealed record StoreReviewsResponse(double Average, int Count, List<StoreReviewRow> Items);
