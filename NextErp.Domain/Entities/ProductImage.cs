namespace NextErp.Domain.Entities
{
    public class ProductImage : IEntity<int>
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string Url { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public bool IsThumbnail { get; set; }

        public string Title { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
