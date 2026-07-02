namespace NextErp.Application.DTOs.Promotion;

public sealed record PromotionConfigDto
{
    public decimal? DiscountAmount { get; init; }
    public decimal? DiscountPercent { get; init; }
    public decimal? MinSubtotal { get; init; }

    public int? ScopeProductId { get; init; }
    public int? ScopeCategoryId { get; init; }
    public int? ScopeProductVariantId { get; init; }

    public List<int>? ScopeProductIds { get; init; }
    public List<int>? ScopeCategoryIds { get; init; }

    public decimal? BuyQuantity { get; init; }
    public decimal? GetQuantity { get; init; }
    public decimal? GetDiscountPercent { get; init; }
    public List<int>? BuyProductIds { get; init; }
    public List<int>? BuyCategoryIds { get; init; }
    public List<int>? GetProductIds { get; init; }
    public decimal? MaxRewardQuantity { get; init; }

    public string? MembershipTier { get; init; }
}
