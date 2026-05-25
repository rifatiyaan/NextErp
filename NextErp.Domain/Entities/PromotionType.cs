namespace NextErp.Domain.Entities;

/// <summary>
/// Classifies a Promotion rule. Drives how the pricing engine interprets
/// the owned PromotionConfig (which fields it reads) and where in the
/// pricing pipeline the promotion is applied.
/// </summary>
public enum PromotionType
{
    /// <summary>Flat or percent off matching line item(s). Applied per-line.</summary>
    LineDiscount = 1,

    /// <summary>Flat or percent off the whole sale. Applied to subtotal after line discounts.</summary>
    InvoiceDiscount = 2,

    /// <summary>"Buy N of product X, get M of product X at Y% off."</summary>
    BogoSame = 3,

    /// <summary>"Buy from set A, get from set B at Y% off." Sets can be products or categories.</summary>
    BogoCross = 4,

    /// <summary>Auto-applied percent based on the customer's MembershipTier.</summary>
    Membership = 5,
}
