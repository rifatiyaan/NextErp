namespace EcommerceApplicationWeb.Domain.Entities
{
    public class ProductStockItem : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public Guid TenantId { get; set; }

        public Guid ProductStockId { get; set; }
        public ProductStock ProductStock { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }

        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public ProductStockItemMetadata Metadata { get; set; } = new ProductStockItemMetadata();

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public class ProductStockItemMetadata
        {
            public string? SupplierInvoiceRef { get; set; }
            public string? Notes { get; set; }
        }
    }
}
