namespace NextErp.Application.DTOs.Tenant;

public sealed record CreateTenantRequest : TenantRequestBase
{
    public bool IsActive { get; init; } = true;
}
