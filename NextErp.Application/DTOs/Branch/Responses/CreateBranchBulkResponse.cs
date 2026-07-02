namespace NextErp.Application.DTOs.Branch;

public sealed record CreateBranchBulkResponse
{
    public List<CreateBranchResponse> Branches { get; init; } = new();
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public List<string> Errors { get; init; } = new();
}
