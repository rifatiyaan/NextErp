using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Module;

public sealed record ModuleResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Icon { get; init; }
    public string? Url { get; init; }
    public int? ParentId { get; init; }
    public ModuleType Type { get; init; }

    // Module-specific
    public string? Description { get; init; }
    public string? Version { get; init; }
    public bool IsInstalled { get; init; }
    public bool IsEnabled { get; init; }
    public DateTime? InstalledAt { get; init; }

    // Common
    public int Order { get; init; }
    public bool IsActive { get; init; }
    public bool IsExternal { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public Guid TenantId { get; init; }

    public ModuleMetadataRequest Metadata { get; init; } = new();
    // set: the query handler assembles the menu tree by populating Children.
    public List<ModuleResponse> Children { get; set; } = new();
}
