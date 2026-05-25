using System.ComponentModel.DataAnnotations.Schema;
using NextErp.Domain.Common;

namespace NextErp.Domain.Entities;

/// <summary>
/// Header of a double-entry bookkeeping transaction. Total debits MUST equal
/// total credits across its <see cref="Lines"/> — that invariant is enforced
/// in the domain helper <see cref="IsBalanced"/> and the create handler.
///
/// Branch-scoped because different branches may run their own books for
/// reporting; the chart-of-accounts itself stays tenant-wide.
/// </summary>
[BranchScoped]
public class JournalEntry : IEntity<Guid>, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }

    /// <summary>Human-readable id (e.g. "JE-2026-0001"), generated server-side.</summary>
    public string EntryNumber { get; set; } = null!;

    /// <summary>IEntity-required; mirrors <see cref="EntryNumber"/> (no separate column).</summary>
    [NotMapped]
    public string Title
    {
        get => EntryNumber;
        set => EntryNumber = value;
    }

    /// <summary>The accounting date — distinct from CreatedAt (which is system insert time).</summary>
    public DateTime EntryDate { get; set; }

    public string Description { get; set; } = null!;

    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;
    public JournalEntryReferenceType ReferenceType { get; set; } = JournalEntryReferenceType.Manual;

    /// <summary>
    /// FK-ish pointer back to the originating business event (sale id,
    /// purchase id, transfer id, …). Stored loosely as Guid? so any source
    /// entity can be referenced without a hard FK that would break with a
    /// table rename.
    /// </summary>
    public Guid? ReferenceId { get; set; }

    /// <summary>External reference / memo, e.g. "Bill #12345" or a bank slip number.</summary>
    public string? Reference { get; set; }

    public Guid CreatedById { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<JournalLine> Lines { get; set; } = new List<JournalLine>();

    /// <summary>
    /// Sum of debits across all lines. Derived; not persisted. Compared with
    /// <see cref="TotalCredit"/> in <see cref="IsBalanced"/>.
    /// </summary>
    public decimal TotalDebit => Lines?.Sum(l => l.Debit) ?? 0;
    public decimal TotalCredit => Lines?.Sum(l => l.Credit) ?? 0;

    /// <summary>
    /// True iff debits exactly equal credits AND there's at least one line.
    /// Use a tiny epsilon so binary-rounded decimals don't trip the check.
    /// </summary>
    public bool IsBalanced => Lines is { Count: > 0 } && Math.Abs(TotalDebit - TotalCredit) < 0.005m;
}
