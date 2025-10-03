namespace EcommerceApplicationWeb.Application.DTOs
{
    public class CategoryRequestDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public CategoryMetadataDto Metadata { get; set; } = null!;
        public bool IsActive { get; set; } = true; // 👈 new
        public int? ParentId { get; internal set; }
    }

    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public CategoryMetadataDto Metadata { get; set; } = null!;
        public List<ProductResponseDto>? Products { get; set; }
        public bool IsActive { get; set; } // 👈 new
        public int? ParentId { get; internal set; }
    }

    public class CategoryMetadataDto
    {
        public string? ProductCount { get; set; }
        public string? Department { get; set; }
    }
}