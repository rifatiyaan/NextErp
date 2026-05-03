namespace NextErp.Domain.Entities;

public class SystemSettings : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "System Settings";
    public Guid TenantId { get; set; }

    // ─── Accent colors — preset XOR custom ───
    public string? PresetAccentTheme { get; set; }

    public string? CustomPrimary { get; set; }
    public string? CustomSecondary { get; set; }
    public string? CustomSidebarBackground { get; set; }
    public string? CustomSidebarForeground { get; set; }

    // ─── Layout ───
    public string NavigationPlacement { get; set; } = "sidebar";

    public string Radius { get; set; } = "md";

    // ─── Branding ───
    public string? CompanyName { get; set; }
    public string? CompanyLogoUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static SystemSettings CreateDefaults(Guid tenantId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        PresetAccentTheme = "theme-slate",
        NavigationPlacement = "sidebar",
        Radius = "md",
        CreatedAt = DateTime.UtcNow,
    };
}

