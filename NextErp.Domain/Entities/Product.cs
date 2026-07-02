using NextErp.Domain.Common;

namespace NextErp.Domain.Entities
{
    [BranchScoped]
    public class Product : IEntity<int>, ISoftDeletable
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int? ParentId { get; set; }
        public Product? Parent { get; set; }
        public ICollection<Product> Children { get; set; } = new List<Product>();

        public decimal Price { get; set; }

        // Average / standard cost — used for profit-margin reporting and stock
        // valuation. Defaults to 0 so existing records remain consistent until
        // costs are backfilled (manually or from purchase order weighted-avg).
        public decimal Cost { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string? ImageUrl { get; set; }
        public ProductMetadataClass Metadata { get; set; } = new ProductMetadataClass();

        public int? UnitOfMeasureId { get; set; }
        public UnitOfMeasure? UnitOfMeasure { get; set; }

        public bool IsActive { get; set; } = true;

        // Storefront curation: only products explicitly published (and whose
        // category is published) appear on the public store.
        public bool IsPublishedOnline { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }

        // Variation system support
        public bool HasVariations { get; set; } = false;

        public ICollection<ProductVariationOption> ProductVariationOptions { get; set; } = new List<ProductVariationOption>();
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

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
