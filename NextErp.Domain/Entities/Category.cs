namespace NextErp.Domain.Entities
{
    public class Category : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();

        public ICollection<Product> Products { get; set; } = new List<Product>();

        public CategoryMetadataClass Metadata { get; set; } = new CategoryMetadataClass();

        public List<CategoryAsset> Assets { get; set; } = new List<CategoryAsset>();

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }

        public class CategoryMetadataClass
        {
            public string? ProductCount { get; set; }
            public string? Department { get; set; }
        }

        public class CategoryAsset
        {
            public string Filename { get; set; } = null!;
            public string Url { get; set; } = null!;
            public string Type { get; set; } = "image"; // image, thumbnail, etc.
            public long? Size { get; set; }
            public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        }
    }
}
