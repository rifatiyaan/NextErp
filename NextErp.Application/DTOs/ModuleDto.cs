namespace NextErp.Application.DTOs
{
    public class ModuleRequestDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Version { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsEnabled { get; set; }
        public ModuleMetadataDto Metadata { get; set; } = new();
    }

    public class ModuleResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Version { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsEnabled { get; set; }
        public ModuleMetadataDto Metadata { get; set; } = new();
    }

    public class ModuleMetadataDto
    {
        public string? Author { get; set; }
        public string? Website { get; set; }
        public string[]? Dependencies { get; set; }
        public string? ConfigurationUrl { get; set; }
    }
}
