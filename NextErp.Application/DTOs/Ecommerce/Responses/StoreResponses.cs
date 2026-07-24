namespace NextErp.Application.DTOs.Ecommerce;

public sealed record StoreHeroSlide(string ImageUrl, string? Headline, string? Subtext, string? Href);
public sealed record StoreConfigResponse(bool StorefrontEnabled, string StoreName, string Tagline, string HeroHeadline, string HeroImageUrl, string MarqueeText, string CodNote, decimal DeliveryFee, List<StoreHeroSlide> HeroSlides, string CurrencyCode, string CurrencyLocale, StoreThemeColors Theme);

// Full resolved storefront palette (all #rrggbb). Injected as CSS custom
// properties on the store scope; every store surface is token-driven so a whole
// palette (including a dark one) can be swapped from one setting.
public sealed record StoreThemeColors(string Ground, string Surface, string Subtle, string Ink, string InkSoft, string Line, string Accent);
public sealed record StoreCategoryResponse(int Id, string Title, int? ParentId, int ProductCount, string? ImageUrl);
public sealed record StoreProductRow(int Id, string Title, decimal Price, string? ImageUrl, string? SecondImageUrl, bool InStock, decimal? LowStockQuantity, bool HasVariations);
public sealed record StorePagedProductsResponse(int Total, List<StoreProductRow> Data);
public sealed record StorePriceRangeResponse(decimal Min, decimal Max);
public sealed record StoreVariantRow(int Id, string Sku, string Title, decimal Price, bool InStock, decimal? LowStockQuantity);
public sealed record StoreProductDetailResponse(int Id, string Title, decimal Price, string? Description, string? CategoryTitle, int CategoryId, List<string> Images, List<StoreVariantRow> Variants);

public sealed record StoreReviewRow(int Id, string AuthorName, int Rating, string Text, DateTime CreatedAt);
public sealed record StoreReviewsResponse(double Average, int Count, List<StoreReviewRow> Items);

// Public order tracking — returned only when order number AND phone match.
public sealed record StoreOrderStatusItem(string ProductTitle, string Sku, decimal UnitPrice, decimal Quantity, decimal LineTotal);
public sealed record StoreOrderStatusResponse(
    string OrderNumber, string Status, DateTime CreatedAt, DateTime? ConfirmedAt,
    decimal DeliveryFee, decimal ItemsTotal, decimal GrandTotal, List<StoreOrderStatusItem> Items);
