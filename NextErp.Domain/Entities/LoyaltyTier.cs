namespace NextErp.Domain.Entities;

/// <summary>
/// Customer membership tier. Computed from lifetime points earned (NOT
/// current balance) — once a customer reaches a tier, redemptions don't
/// demote them. Thresholds are enforced in the LoyaltyService at compute
/// time so they can be tuned without a migration.
/// </summary>
public enum LoyaltyTier
{
    None = 0,
    Bronze = 1,
    Silver = 2,
    Gold = 3,
    Platinum = 4,
}
