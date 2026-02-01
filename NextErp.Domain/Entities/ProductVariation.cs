namespace NextErp.Domain.Entities
{
    public class ProductVariation : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!; // Required by IEntity - combination of Name and Value
        public string Name { get; set; } = null!; // e.g., "Size", "Color", "Material"
        public string Value { get; set; } = null!; // e.g., "Large", "Red", "Cotton"
        public decimal? PriceAdjustment { get; set; } // Optional price adjustment for this variation
        public int? Stock { get; set; } // Optional stock for this specific variation
        public string? Sku { get; set; } // Optional SKU for this variation
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }

        // Many-to-many relationship with Products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

