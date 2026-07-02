namespace NextErp.Application.DTOs.Tenant;

public sealed record CreateTenantResponse : TenantResponseBase
{
    public TenantMetadataRequest Metadata { get; init; } = new();
}
