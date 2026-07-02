namespace NextErp.Application.DTOs.SystemSettings;

public sealed record UpdateSystemSettingsRequest
{
    // Either preset OR custom — validator enforces XOR.
    public string? PresetAccentTheme { get; init; }
    public string? CustomPrimary { get; init; }
    public string? CustomSecondary { get; init; }
    public string? CustomSidebarBackground { get; init; }
    public string? CustomSidebarForeground { get; init; }

    public string? NavigationPlacement { get; init; }
    public string? NavigationShape { get; init; }
    public string? Radius { get; init; }

    public string? CompanyName { get; init; }
    public string? CompanyLogoUrl { get; init; }
}
