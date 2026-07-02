namespace NextErp.Application.DTOs.Module;

public sealed record ModuleMetadataRequest
{
    // From MenuItem
    public string[]? Roles { get; init; }
    public string? BadgeText { get; init; }
    public string? BadgeColor { get; init; }
    public string? Description { get; init; }
    public bool OpenInNewTab { get; init; }

    // From Module
    public string? Author { get; init; }
    public string? Website { get; init; }
    public string[]? Dependencies { get; init; }
    public string? ConfigurationUrl { get; init; }
}
