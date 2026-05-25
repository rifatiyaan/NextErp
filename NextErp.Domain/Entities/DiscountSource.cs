namespace NextErp.Domain.Entities;

/// <summary>
/// Audit hint on a Sale/SaleItem/Purchase/PurchaseItem: did this discount
/// come from an operator typing a value, or from the rule engine auto-
/// applying a Promotion? Used for reporting + UI badge rendering.
/// </summary>
public enum DiscountSource
{
    /// <summary>Operator entered the discount at sale time.</summary>
    Manual = 1,

    /// <summary>Auto-applied by the pricing engine via a Promotion rule.</summary>
    Promotion = 2,
}
