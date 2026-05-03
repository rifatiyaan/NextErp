namespace NextErp.Domain.Entities;

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

