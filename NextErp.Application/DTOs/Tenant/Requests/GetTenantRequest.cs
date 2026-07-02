namespace NextErp.Application.DTOs.Tenant;

public sealed record GetTenantRequest
{
    public Guid Id { get; init; }
}
