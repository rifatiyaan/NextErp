using System.ComponentModel.DataAnnotations.Schema;
using NextErp.Domain.Common;

namespace NextErp.Domain.Entities;

/// <summary>
/// Append-only ledger of customer loyalty points. Positive <see cref="Points"/>
/// = earn (purchase, manual credit, refund); negative = redemption or
/// expiry. Customer's current balance is the SUM across all active rows;
/// lifetime earned is SUM of positive rows (used to compute tier).
///
/// Branch-scoped because a customer may earn at one branch and redeem at
/// another — both rows are filterable per branch for reporting, even
/// though balance + tier are computed per-customer across the tenant.
/// </summary>
[BranchScoped]
public class LoyaltyTransaction : IEntity<Guid>, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }

    /// <summary>The customer's Party id. Should be a Customer-typed Party.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Signed integer — positive = earn, negative = redeem/expire.</summary>
    public int Points { get; set; }

    public LoyaltyTransactionReason Reason { get; set; }

    /// <summary>
    /// Loose pointer to the originating event (sale id when Reason=PurchaseEarn,
    /// sale return id when Reason=Refund, etc.). Stored as Guid? rather than
    /// a hard FK so any source entity can be referenced.
    /// </summary>
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }

    /// <summary>Free-form note (e.g. "Birthday bonus", "Customer service goodwill").</summary>
    public string? Notes { get; set; }

    /// <summary>IEntity-required; synthesises a label from Reason + Points (no separate column).</summary>
    [NotMapped]
    public string Title
    {
        get => $"{Reason} ({(Points >= 0 ? "+" : "")}{Points})";
        set { /* no-op — read-only label */ }
    }

    public Guid CreatedById { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
