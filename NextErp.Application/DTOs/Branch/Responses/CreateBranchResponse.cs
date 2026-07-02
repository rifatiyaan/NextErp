namespace NextErp.Application.DTOs.Branch;

public sealed record CreateBranchResponse : BranchResponseBase
{
    public BranchMetadataRequest Metadata { get; init; } = new();
}
