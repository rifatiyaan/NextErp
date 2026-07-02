namespace NextErp.Application.DTOs.Branch;

public sealed record UpdateBranchBulkResponse
{
    public List<UpdateBranchResponse> Branches { get; init; } = new();
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public List<string> Errors { get; init; } = new();
}
