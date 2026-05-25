using System.ComponentModel.DataAnnotations.Schema;
using NextErp.Domain.Common;

namespace NextErp.Domain.Entities;

/// <summary>
/// One side of a double-entry journal posting — a debit OR credit hit on a
/// single <see cref="Account"/>. Inherits its branch + tenant scope from
/// the parent <see cref="JournalEntry"/>; no [BranchScoped] needed.
/// </summary>
public class JournalLine : IEntity<Guid>, ISoftDeletable
{
    public Guid Id { get; set; }

    public Guid JournalEntryId { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    /// <summary>Optional per-line description; falls back to the parent entry's description in reports.</summary>
    public string? Description { get; set; }

    /// <summary>IEntity-required; mirrors <see cref="Description"/> (no separate column).</summary>
    [NotMapped]
    public string Title
    {
        get => Description ?? string.Empty;
        set => Description = value;
    }

    /// <summary>
    /// Debit amount. A line is *either* debit or credit — exactly one of
    /// the two must be non-zero. Validation is enforced in the create
    /// handler so the DB row layout stays simple (no XOR check constraint).
    /// </summary>
    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    /// <summary>Display order within the entry (small integer, 1-based).</summary>
    public int LineOrder { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
