using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Accounting;

public sealed record JournalResponse
{
    public Guid Id { get; init; }
    public string EntryNumber { get; init; } = null!;
    public DateTime EntryDate { get; init; }
    public string Description { get; init; } = null!;
    public JournalEntryStatus Status { get; init; }
    public string StatusName => Status.ToString();
    public JournalEntryReferenceType ReferenceType { get; init; }
    public string ReferenceTypeName => ReferenceType.ToString();
    public Guid? ReferenceId { get; init; }
    public string? Reference { get; init; }
    public DateTime CreatedAt { get; init; }
    public decimal TotalDebit { get; init; }
    public decimal TotalCredit { get; init; }
    public List<JournalLineResponse> Lines { get; init; } = new();
}
