using NextErp.Application.DTOs.Tenant;

namespace NextErp.Application.DTOs.Branch;

public sealed record BranchResponse : BranchResponseBase
{
    public BranchMetadataRequest Metadata { get; init; } = new();
    public TenantResponse? Tenant { get; init; }
}
