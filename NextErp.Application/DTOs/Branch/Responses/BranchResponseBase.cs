namespace NextErp.Application.DTOs.Branch;

public abstract record BranchResponseBase
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = null!;
    public string? Address { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
