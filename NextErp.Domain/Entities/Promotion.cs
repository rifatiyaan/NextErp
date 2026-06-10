using System.ComponentModel.DataAnnotations.Schema;
using NextErp.Domain.Common;

namespace NextErp.Domain.Entities;

/// <summary>
/// A discount/promotion rule. The Config (owned JSON) carries the
/// type-specific fields — the pricing engine reads it based on Type at
/// sale time. Tenant-wide (no BranchScoped); if a tenant ever wants
/// branch-specific promotions, add an optional BranchId column.
///
/// Audit: see IsActive (soft delete) + Stackable + Priority for stacking
/// semantics. Date gates (StartDate/EndDate) are nullable so a permanent
/// rule (e.g. VIP tier) doesn't need a sentinel value.
/// </summary>
public class Promotion : IEntity<Guid>, ISoftDeletable
{
    public Guid Id { get; set; }

    /// <summary>Human-readable label shown in admin + as the pill on sale receipts.</summary>
    public string Name { get; set; } = null!;

    /// <summary>IEntity contract requirement; mirrors <see cref="Name"/>.</summary>
    [NotMapped]
    public string Title
    {
        get => Name;
        set => Name = value;
    }

    public string? Description { get; set; }

    public PromotionType Type { get; set; }

    /// <summary>Owned JSON column — type-specific config fields.</summary>
    public PromotionConfig Config { get; set; } = new();

    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Higher value applied first when stacking. Default 0 (lowest priority).
    /// Tie-breaks fall back to CreatedAt ASC (oldest first).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// When false this promotion is exclusive — if it applies to a scope
    /// (line or invoice), no other promotion of the same Type can apply
    /// there. Manual operator discounts always stack on top regardless.
    /// </summary>
    public bool Stackable { get; set; } = true;

    public Guid TenantId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
}

/// <summary>
/// Owned JSON config for a Promotion. Wide nullable shape — each
/// PromotionType reads its own subset; FluentValidation enforces "for
/// type X, fields a/b/c required, others must be null" at the API edge
/// so invalid combinations never reach the database.
/// </summary>
public class PromotionConfig
{
    // Discount values (Line/Invoice/Bogo all reuse one of these)
    public decimal? DiscountAmount { get; set; }    // flat $
    public decimal? DiscountPercent { get; set; }   // % (0-100)

    // Invoice gate
    public decimal? MinSubtotal { get; set; }

    // Line / Invoice scope filter — which lines or sales are eligible.
    // Singular fields kept for backward compatibility with the early
    // sentence-builder UI. Plural ScopeProductIds/ScopeCategoryIds is
    // the canonical multi-select form used by the new bulk-picker UI
    // and is the path forward; the pricing engine checks both.
    public int? ScopeProductId { get; set; }
    public int? ScopeCategoryId { get; set; }
    public int? ScopeProductVariantId { get; set; }
    public List<int>? ScopeProductIds { get; set; }
    public List<int>? ScopeCategoryIds { get; set; }

    // BOGO mechanics. BUY set qualifies the cart; GET products are auto-added
    // as bonus lines. Same-product BOGO = GetProductIds holds the buy product.
    public decimal? BuyQuantity { get; set; }        // N — buy this many to earn a set
    public decimal? GetQuantity { get; set; }        // M — free/discounted units of EACH get product per set
    public decimal? GetDiscountPercent { get; set; } // 100 = free, 50 = half-off
    public List<int>? BuyProductIds { get; set; }
    public List<int>? BuyCategoryIds { get; set; }
    public List<int>? GetProductIds { get; set; }
    // Optional ceiling on reward units per GET product across the whole cart
    // (e.g. "buy 2, get 1 free — up to 5"). Null = uncapped.
    public decimal? MaxRewardQuantity { get; set; }

    // Membership match key — joins Party.MembershipTier
    public string? MembershipTier { get; set; }
}
