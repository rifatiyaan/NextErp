namespace NextErp.Application.DTOs;

public class SystemSettings
{
    public class Response
    {
        public class Single
        {
            public Guid Id { get; set; }
            public Guid TenantId { get; set; }

            public string? PresetAccentTheme { get; set; }
            public string? CustomPrimary { get; set; }
            public string? CustomSecondary { get; set; }
            public string? CustomSidebarBackground { get; set; }
            public string? CustomSidebarForeground { get; set; }

            public string NavigationPlacement { get; set; } = "sidebar";
            public string Radius { get; set; } = "md";

            public string? CompanyName { get; set; }
            public string? CompanyLogoUrl { get; set; }

            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
    }

    public class Request
    {
        public class Update
        {
            // Either preset OR custom — validator enforces XOR.
            public string? PresetAccentTheme { get; set; }
            public string? CustomPrimary { get; set; }
            public string? CustomSecondary { get; set; }
            public string? CustomSidebarBackground { get; set; }
            public string? CustomSidebarForeground { get; set; }

            public string? NavigationPlacement { get; set; }
            public string? Radius { get; set; }

            public string? CompanyName { get; set; }
            public string? CompanyLogoUrl { get; set; }
        }
    }
}
