namespace EcommerceApplicationWeb.Domain.Entities
{
    public class Module : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Version { get; set; }
        public string? IconUrl { get; set; }

        public bool IsInstalled { get; set; }
        public bool IsEnabled { get; set; }

        public DateTime? InstalledAt { get; set; }

        public ModuleMetadata Metadata { get; set; } = new ModuleMetadata();

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        public Guid TenantId { get; set; }
        public DateTime CreatedAt { get; set; }

        public class ModuleMetadata
        {
            public string? Author { get; set; }
            public string? Website { get; set; }
            public string[]? Dependencies { get; set; }
            public string? ConfigurationUrl { get; set; }
        }
    }
}
