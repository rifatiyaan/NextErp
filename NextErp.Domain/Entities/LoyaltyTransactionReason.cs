namespace NextErp.Domain.Entities;

/// <summary>
/// Why a <see cref="LoyaltyTransaction"/> was recorded. Drives reporting
/// (how many points came from purchases vs. manual adjustments) and
/// expiry policy (only PurchaseEarn is subject to expiry).
/// </summary>
public enum LoyaltyTransactionReason
{
    PurchaseEarn = 1,
    Redemption = 2,
    ManualAdjust = 3,
    Expired = 4,
    Refund = 5,
}
