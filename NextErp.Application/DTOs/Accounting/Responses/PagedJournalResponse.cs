namespace NextErp.Application.DTOs.Accounting;

public sealed record PagedJournalResponse
{
    public int Total { get; init; }
    public int TotalDisplay { get; init; }
    public List<JournalListRowResponse> Data { get; init; } = new();
}
