using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Promotion;

public partial class PromotionDto
{
    public partial class Request
    {
        public class Create
        {
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public PromotionType Type { get; set; }
            public ConfigDto Config { get; set; } = new();
            public bool IsActive { get; set; } = true;
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int Priority { get; set; }
            public bool Stackable { get; set; } = true;
        }

        public class Update
        {
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public PromotionType Type { get; set; }
            public ConfigDto Config { get; set; } = new();
            public bool IsActive { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int Priority { get; set; }
            public bool Stackable { get; set; }
        }

        /// <summary>
        /// Mirror of <see cref="PromotionConfig"/> — same wide nullable shape.
        /// FluentValidation enforces "for type X, required fields are a/b/c"
        /// at the API edge so invalid combos never reach the DB.
        /// </summary>
        public class ConfigDto
        {
            public decimal? DiscountAmount { get; set; }
            public decimal? DiscountPercent { get; set; }
            public decimal? MinSubtotal { get; set; }
            public int? ScopeProductId { get; set; }
            public int? ScopeCategoryId { get; set; }
            public int? ScopeProductVariantId { get; set; }
            // Multi-select scope — canonical path from the bulk-picker UI.
            // Singular fields above kept for backward compatibility.
            public List<int>? ScopeProductIds { get; set; }
            public List<int>? ScopeCategoryIds { get; set; }
            public decimal? BuyQuantity { get; set; }
            public decimal? GetQuantity { get; set; }
            public decimal? GetDiscountPercent { get; set; }
            public List<int>? BuyProductIds { get; set; }
            public List<int>? BuyCategoryIds { get; set; }
            public List<int>? GetProductIds { get; set; }
            public decimal? MaxRewardQuantity { get; set; }
            public string? MembershipTier { get; set; }
        }
    }

    public partial class Response
    {
        public class Single
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public PromotionType Type { get; set; }
            public string TypeName => Type.ToString();
            public Request.ConfigDto Config { get; set; } = new();
            public bool IsActive { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int Priority { get; set; }
            public bool Stackable { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }

        public class Paged
        {
            public int Total { get; set; }
            public int TotalDisplay { get; set; }
            public List<Single> Data { get; set; } = new();
        }
    }
}
