namespace NextErp.Application.DTOs.Accounting;

/// <summary>
/// Account-to-account transfer. Backend converts this to a balanced
/// JournalEntry with two lines (debit destination, credit source).
/// </summary>
public sealed record TransferJournalRequest
{
    public Guid FromAccountId { get; init; }
    public Guid ToAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Description { get; init; } = null!;
    public DateTime? EntryDate { get; init; }
    public string? Reference { get; init; }
}
