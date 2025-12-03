namespace EcommerceApplicationWeb.Domain.Entities
{
    public class MenuItem : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public int? ParentId { get; set; }
        public MenuItem? Parent { get; set; }
        public ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();

        public Guid? ModuleId { get; set; }
        public Module? Module { get; set; }

        public int Order { get; set; }
        public MenuItemMetadata Metadata { get; set; } = new MenuItemMetadata();

        public bool IsActive { get; set; } = true;
        public bool IsExternal { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }

        public class MenuItemMetadata
        {
            public string[]? Roles { get; set; }
            public string? BadgeText { get; set; }
            public string? BadgeColor { get; set; }
            public string? Description { get; set; }
            public bool OpenInNewTab { get; set; }
        }
    }
}
