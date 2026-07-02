namespace NextErp.Application.DTOs.Branch;

public sealed record CreateBranchRequest : BranchRequestBase
{
    public bool IsActive { get; init; } = true;
}
