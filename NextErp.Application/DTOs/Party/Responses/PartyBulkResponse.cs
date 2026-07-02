namespace NextErp.Application.DTOs.Party;

public sealed record PartyBulkResponse
{
    public List<PartyResponse> Records { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
