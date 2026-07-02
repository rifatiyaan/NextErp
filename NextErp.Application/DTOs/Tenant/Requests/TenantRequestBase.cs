namespace NextErp.Application.DTOs.Tenant;

public abstract record TenantRequestBase
{
    public string Name { get; init; } = null!;
    public string? DatabaseConnectionString { get; init; }
    public TenantMetadataRequest Metadata { get; init; } = new();
}
