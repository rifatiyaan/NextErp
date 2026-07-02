using NextErp.Application.DTOs.Branch;

namespace NextErp.Application.DTOs.Tenant;

public sealed record TenantResponse : TenantResponseBase
{
    public TenantMetadataRequest Metadata { get; init; } = new();
    public List<BranchResponse>? Branches { get; init; }
}
