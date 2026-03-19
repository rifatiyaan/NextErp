namespace NextErp.Domain.Entities
{
    public class ProductVariationOption : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!; // Required by IEntity; can duplicate VariationOption.Name for display
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int VariationOptionId { get; set; }
        public VariationOption VariationOption { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
