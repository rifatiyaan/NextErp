namespace EcommerceApplicationWeb.Application.DTOs
{
    public class MenuItemRequestDto
    {
        public string Title { get; set; } = null!;
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public int? ParentId { get; set; }
        public int Order { get; set; }
        public bool IsExternal { get; set; }
        public Guid? ModuleId { get; set; }
        public MenuItemMetadataDto Metadata { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    public class MenuItemResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public int? ParentId { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public bool IsExternal { get; set; }
        public MenuItemMetadataDto Metadata { get; set; } = new();
        public List<MenuItemResponseDto> Children { get; set; } = new();
    }

    public class MenuItemMetadataDto
    {
        public string[]? Roles { get; set; }
        public string? BadgeText { get; set; }
        public string? BadgeColor { get; set; }
        public string? Description { get; set; }
        public bool OpenInNewTab { get; set; }
    }
}
