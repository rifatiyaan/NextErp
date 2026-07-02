namespace NextErp.Application.DTOs.Tenant;

public sealed record UpdateTenantResponse : TenantResponseBase
{
    public TenantMetadataRequest Metadata { get; init; } = new();
}
