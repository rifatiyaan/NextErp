namespace NextErp.Application.DTOs.Branch;

public sealed record UpdateBranchResponse : BranchResponseBase
{
    public BranchMetadataRequest Metadata { get; init; } = new();
}
