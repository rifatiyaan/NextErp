namespace NextErp.Domain.Entities;

/// <summary>
/// Lifecycle of a <see cref="JournalEntry"/>. Posted is the canonical
/// "this affects balances" state; Draft holds entries pending review;
/// Voided keeps the row for audit but excludes it from totals.
/// </summary>
public enum JournalEntryStatus
{
    Draft = 1,
    Posted = 2,
    Voided = 3,
}
