namespace NextErp.Application.DTOs.Branch;

public sealed record BranchMetadataRequest
{
    public string? Phone { get; init; }
    public string? ManagerName { get; init; }
    public string? BranchCode { get; init; }
    public string? Email { get; init; }
}
