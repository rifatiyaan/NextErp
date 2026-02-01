namespace NextErp.Domain.Entities
{
    public class ProductVariant : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!; // Auto-generated from variation values, e.g., "S / Red"
        public string Name { get; set; } = null!; // Same as Title
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        // Variant-specific properties
        public string Sku { get; set; } = null!; // Unique SKU for this variant
        public decimal Price { get; set; } // Price for this specific variant
        public int Stock { get; set; } // Stock quantity for this variant
        public bool IsActive { get; set; } = true; // Status
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }
        
        // Many-to-many relationship with VariationValue
        public ICollection<VariationValue> VariationValues { get; set; } = new List<VariationValue>();
    }
}

