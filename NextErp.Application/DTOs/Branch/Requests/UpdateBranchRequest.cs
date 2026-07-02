namespace NextErp.Application.DTOs.Branch;

public sealed record UpdateBranchRequest : BranchRequestBase
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; } = true;
}
