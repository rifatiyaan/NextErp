namespace NextErp.Application.DTOs.Tenant;

public sealed record UpdateTenantRequest : TenantRequestBase
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; } = true;
}
