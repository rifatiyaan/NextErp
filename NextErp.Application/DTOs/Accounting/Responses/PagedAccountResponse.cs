namespace NextErp.Application.DTOs.Accounting;

public sealed record PagedAccountResponse
{
    public int Total { get; init; }
    public int TotalDisplay { get; init; }
    public List<AccountResponse> Data { get; init; } = new();
}
