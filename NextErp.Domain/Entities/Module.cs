namespace NextErp.Domain.Entities
{
    public enum ModuleType
    {
        Module = 1,    // Top-level module/feature package
        Link = 2       // Menu link/navigation item
    }

    public class Module : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Icon { get; set; }
        public string? Url { get; set; }
        
        // Hierarchy - self-referencing
        public int? ParentId { get; set; }
        public Module? Parent { get; set; }
        public ICollection<Module> Children { get; set; } = new List<Module>();

        // Type discriminator
        public ModuleType Type { get; set; } = ModuleType.Link;

        // Module-specific properties (used when Type = Module)
        public string? Description { get; set; }
        public string? Version { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? InstalledAt { get; set; }

        // Common properties
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsExternal { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }

        // Metadata (merged from both MenuItem and Module)
        public ModuleMetadata Metadata { get; set; } = new ModuleMetadata();

        public class ModuleMetadata
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
}
