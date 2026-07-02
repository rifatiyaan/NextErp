namespace NextErp.Application.DTOs.Tenant;

public abstract record TenantResponseBase
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? DatabaseConnectionString { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
