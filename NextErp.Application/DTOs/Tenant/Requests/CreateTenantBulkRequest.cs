namespace NextErp.Application.DTOs.Tenant;

public sealed record CreateTenantBulkRequest
{
    public List<CreateTenantRequest> Tenants { get; init; } = new();
}
