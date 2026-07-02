namespace NextErp.Application.DTOs.Branch;

public sealed record CreateBranchBulkRequest
{
    public List<CreateBranchRequest> Branches { get; init; } = new();
}
