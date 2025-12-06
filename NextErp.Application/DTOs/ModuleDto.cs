namespace NextErp.Application.DTOs
{
    public class ModuleRequestDto
    {
        public string Title { get; set; } = null!;
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public int? ParentId { get; set; }
        public int Type { get; set; } = 2; // Default to Link
        
        // Module-specific (used when Type = Module)
        public string? Description { get; set; }
        public string? Version { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsEnabled { get; set; }
        
        // Common
        public int Order { get; set; }
        public bool IsExternal { get; set; }
        public bool IsActive { get; set; } = true;
        
        public ModuleMetadataDto Metadata { get; set; } = new();
    }

    public class ModuleResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public int? ParentId { get; set; }
        public int Type { get; set; }
        
        // Module-specific
        public string? Description { get; set; }
        public string? Version { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? InstalledAt { get; set; }
        
        // Common
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public bool IsExternal { get; set; }
        
        public ModuleMetadataDto Metadata { get; set; } = new();
        public List<ModuleResponseDto> Children { get; set; } = new();
    }

    public class ModuleMetadataDto
    {
        // From MenuItem
        public string[]? Roles { get; set; }
        public string? BadgeText { get; set; }
        public string? BadgeColor { get; set; }
        public string? Description { get; set; }
        public bool OpenInNewTab { get; set; }

        // From Module
        public string? Author { get; set; }
        public string? Website { get; set; }
        public string[]? Dependencies { get; set; }
        public string? ConfigurationUrl { get; set; }
    }
}
