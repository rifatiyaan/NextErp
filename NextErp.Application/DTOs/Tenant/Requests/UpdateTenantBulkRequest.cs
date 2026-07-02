namespace NextErp.Application.DTOs.Tenant;

public sealed record UpdateTenantBulkRequest
{
    public List<UpdateTenantRequest> Tenants { get; init; } = new();
}
