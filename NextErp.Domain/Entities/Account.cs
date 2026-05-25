using System.ComponentModel.DataAnnotations.Schema;
using NextErp.Domain.Common;

namespace NextErp.Domain.Entities;

/// <summary>
/// A node in the tenant's chart of accounts. Tenant-scoped (NOT branch-scoped)
/// because the CoA is shared across all branches — individual transactions
/// (<see cref="JournalEntry"/>) carry their own BranchId for filtering.
///
/// Hierarchy: ParentAccountId allows nested grouping (e.g. "1000 Assets" →
/// "1100 Current assets" → "1110 Cash on hand"). Leaf accounts hold the
/// actual postings; parents aggregate balances on the trial balance.
/// </summary>
public class Account : IEntity<Guid>, ISoftDeletable
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>Short numeric/alphanumeric code unique within the tenant (e.g. "1110").</summary>
    public string Code { get; set; } = null!;

    /// <summary>Human-readable name (e.g. "Cash on hand").</summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// IEntity-required field; we don't persist a separate column — it just
    /// mirrors <see cref="Name"/> so existing code that reads Title
    /// generically (logging, audit) sees a sensible value.
    /// </summary>
    [NotMapped]
    public string Title
    {
        get => Name;
        set => Name = value;
    }

    public AccountType Type { get; set; }

    /// <summary>Parent in the chart-of-accounts tree. Null for top-level accounts.</summary>
    public Guid? ParentAccountId { get; set; }
    public Account? ParentAccount { get; set; }
    public ICollection<Account> Children { get; set; } = new List<Account>();

    /// <summary>
    /// When false, no new postings can target this account. Existing
    /// transactions remain so historical statements aren't disturbed.
    /// </summary>
    public bool IsPostingAllowed { get; set; } = true;

    /// <summary>Free-form note shown in CoA admin (e.g. "Petty cash, $200 float").</summary>
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<JournalLine> JournalLines { get; set; } = new List<JournalLine>();
}
