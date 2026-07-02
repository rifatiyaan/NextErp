namespace NextErp.Application.DTOs.Branch;

public sealed record BranchBulkResponse
{
    public List<BranchResponse> Branches { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
