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

    /// <summary>
    /// "Buy N from the BUY set, get M of each GET product at Y% off." The GET
    /// products are auto-added to the order as bonus lines. Same-product BOGO
    /// is just the case where the GET list equals the BUY product.
    /// </summary>
    Bogo = 3,

    /// <summary>Auto-applied percent based on the customer's MembershipTier.</summary>
    Membership = 5,
}
