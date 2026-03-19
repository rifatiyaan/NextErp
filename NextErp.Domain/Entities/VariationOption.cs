namespace NextErp.Domain.Entities
{
    public class VariationOption : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }

        public ICollection<VariationValue> Values { get; set; } = new List<VariationValue>();
        public ICollection<ProductVariationOption> ProductVariationOptions { get; set; } = new List<ProductVariationOption>();
    }
}
