namespace NextErp.Application.DTOs.Sale;

public sealed record GetSalesRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public Guid? PartyId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}
