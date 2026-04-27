namespace NextErp.Domain.Entities;

/// <summary>How a manual stock adjustment's quantity is interpreted.</summary>
public enum StockAdjustmentMode
{
    Increase = 1,
    Decrease = 2,
    SetAbsolute = 3
}
