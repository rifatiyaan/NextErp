namespace NextErp.Domain.Entities;

/// <summary>Canonical reason codes for manual stock adjustments. Stored as strings on <see cref="StockMovement.Reason"/>.</summary>
public static class StockAdjustmentReason
{
    public const string PhysicalCountCorrection = "PhysicalCountCorrection";
    public const string Damaged = "Damaged";
    public const string Expired = "Expired";
    public const string LostOrTheft = "LostOrTheft";
    public const string OpeningBalance = "OpeningBalance";
    public const string DataEntryCorrection = "DataEntryCorrection";
    public const string Other = "Other";

    public static readonly IReadOnlyList<string> All = new[]
    {
        PhysicalCountCorrection,
        Damaged,
        Expired,
        LostOrTheft,
        OpeningBalance,
        DataEntryCorrection,
        Other
    };
}
