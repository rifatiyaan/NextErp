namespace NextErp.Domain.Entities
{
    public class VariationValue : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!; // e.g., "S", "Red", "Cotton"
        public string Name { get; set; } = null!; // Same as Title, required by IEntity
        public string Value { get; set; } = null!; // Same as Title, for consistency
        
        public int VariationOptionId { get; set; }
        public VariationOption VariationOption { get; set; } = null!;
        
        // Display order for UI
        public int DisplayOrder { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }
        
        // Many-to-many relationship with ProductVariant
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    }
}

