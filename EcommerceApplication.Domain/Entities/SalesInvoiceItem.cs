namespace EcommerceApplicationWeb.Domain.Entities
{
    public class SalesInvoiceItem : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public Guid TenantId { get; set; }

        public Guid SalesInvoiceId { get; set; }
        public SalesInvoice SalesInvoice { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public Guid? WarehouseId { get; set; }
        //public Warehouse? Warehouse { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Total => (Quantity * UnitPrice) - Discount + ((Quantity * UnitPrice) * (TaxRate / 100));

        public SalesInvoiceItemMetadata Metadata { get; set; } = new SalesInvoiceItemMetadata();

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public class SalesInvoiceItemMetadata
        {
            public string? BatchNumber { get; set; }
            public string? SerialNumber { get; set; }
            public string? Notes { get; set; }
        }
    }
}
