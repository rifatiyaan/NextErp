namespace NextErp.Domain.Entities
{
    public class VariationOption : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!; // e.g., "Size", "Color", "Material"
        public string Name { get; set; } = null!; // Same as Title, required by IEntity
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        // Display order for UI
        public int DisplayOrder { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }
        
        // Collection of values for this option
        public ICollection<VariationValue> Values { get; set; } = new List<VariationValue>();
    }
}

