namespace NextErp.Application.DTOs.Accounting;

public sealed record JournalLineResponse
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public string AccountCode { get; init; } = null!;
    public string AccountName { get; init; } = null!;
    public string? Description { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public int LineOrder { get; init; }
}
