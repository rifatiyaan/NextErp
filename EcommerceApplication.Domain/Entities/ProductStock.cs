namespace EcommerceApplicationWeb.Domain.Entities
{

    public class ProductStock : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public Guid TenantId { get; set; }

        public Guid WarehouseId { get; set; }
        //public Warehouse Warehouse { get; set; } = null!;

        // Total Quantity for this stock summary
        public decimal TotalQuantity { get; set; }

        public ICollection<ProductStockItem> Items { get; set; } = new List<ProductStockItem>();

        public ProductStockMetadata Metadata { get; set; } = new ProductStockMetadata();

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public class ProductStockMetadata
        {
            public string? Notes { get; set; }
        }
    }
}
