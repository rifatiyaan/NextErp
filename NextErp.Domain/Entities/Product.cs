namespace NextErp.Domain.Entities
{
    public class Product : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int? ParentId { get; set; }
        public Product? Parent { get; set; }
        public ICollection<Product> Children { get; set; } = new List<Product>();

        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string? ImageUrl { get; set; }
        public ProductMetadataClass Metadata { get; set; } = new ProductMetadataClass();

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }

        // Variation system support
        public bool HasVariations { get; set; } = false; // Flag to indicate if product has variations
        
        // New variation system (VariationOption -> VariationValue -> ProductVariant)
        public ICollection<VariationOption> VariationOptions { get; set; } = new List<VariationOption>();
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
        
        // Legacy: Many-to-many relationship with ProductVariation (kept for backward compatibility)
        public ICollection<ProductVariation> Variations { get; set; } = new List<ProductVariation>();

        public class ProductMetadataClass
        {
            public string? Description { get; set; }
            public string? Color { get; set; }
            public string? Warranty { get; set; }
        }
    }
}
