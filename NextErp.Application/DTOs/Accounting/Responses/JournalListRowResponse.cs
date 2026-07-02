using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Accounting;

public sealed record JournalListRowResponse
{
    public Guid Id { get; init; }
    public string EntryNumber { get; init; } = null!;
    public DateTime EntryDate { get; init; }
    public string Description { get; init; } = null!;
    public JournalEntryReferenceType ReferenceType { get; init; }
    public string ReferenceTypeName => ReferenceType.ToString();
    public string? Reference { get; init; }
    public decimal TotalAmount { get; init; }
    public int LineCount { get; init; }
    /// <summary>First non-zero-debit line — handy for "From / To" display in transfer rows.</summary>
    public string? FromAccount { get; init; }
    /// <summary>First non-zero-credit line — handy for "From / To" display in transfer rows.</summary>
    public string? ToAccount { get; init; }
}
