namespace NextErp.Application.DTOs.SystemSettings;

public sealed record SystemSettingsResponse
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }

    public string? PresetAccentTheme { get; init; }
    public string? CustomPrimary { get; init; }
    public string? CustomSecondary { get; init; }
    public string? CustomSidebarBackground { get; init; }
    public string? CustomSidebarForeground { get; init; }

    public string NavigationPlacement { get; init; } = "sidebar";
    public string NavigationShape { get; init; } = "flat";
    public string Radius { get; init; } = "md";

    public string? CompanyName { get; init; }
    public string? CompanyLogoUrl { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
