using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs
{
    public class BulkModuleDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Version { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? InstalledAt { get; set; }
        public ModuleMetadataDto? Metadata { get; set; }
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public bool IsExternal { get; set; }
        public ModuleType Type { get; set; }
        
        // Recursive property for children
        public List<BulkModuleDto> Children { get; set; } = new();
    }
}
