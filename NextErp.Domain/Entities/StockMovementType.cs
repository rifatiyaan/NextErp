namespace NextErp.Domain.Entities;

public enum StockMovementType
{
    Sale = 0,
    Purchase = 1,
    /// <summary>Manual correction (UI/API adjustments). Formerly <c>Adjustment</c> (same underlying value 2).</summary>
    ManualAdjustment = 2,
    Return = 3,
    Transfer = 4
}
