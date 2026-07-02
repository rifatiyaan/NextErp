namespace NextErp.Application.DTOs.Branch;

public sealed record UpdateBranchBulkRequest
{
    public List<UpdateBranchRequest> Branches { get; init; } = new();
}
