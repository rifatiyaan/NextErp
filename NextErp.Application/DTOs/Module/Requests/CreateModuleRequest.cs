using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Module;

public sealed record CreateModuleRequest
{
    public string Title { get; init; } = null!;
    public string? Icon { get; init; }
    public string? Url { get; init; }
    public int? ParentId { get; init; }
    public ModuleType Type { get; init; } = ModuleType.Link;

    // Module-specific (used when Type = Module)
    public string? Description { get; init; }
    public string? Version { get; init; }
    public bool IsInstalled { get; init; }
    public bool IsEnabled { get; init; }

    // Common
    public int Order { get; init; }
    public bool IsExternal { get; init; }

    public ModuleMetadataRequest Metadata { get; init; } = new();

    public bool IsActive { get; init; } = true;
}
