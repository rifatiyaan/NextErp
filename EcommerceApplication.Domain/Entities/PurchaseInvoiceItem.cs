namespace EcommerceApplicationWeb.Domain.Entities
{
    public class PurchaseInvoiceItem : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public Guid TenantId { get; set; }

        public Guid PurchaseInvoiceId { get; set; }
        public PurchaseInvoice PurchaseInvoice { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public Guid? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Total => (Quantity * UnitCost) - Discount + ((Quantity * UnitCost) * (TaxRate / 100));

        public PurchaseInvoiceItemMetadata Metadata { get; set; } = new PurchaseInvoiceItemMetadata();

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public class PurchaseInvoiceItemMetadata
        {
            public string? BatchNumber { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public string? SupplierInvoiceRef { get; set; }
            public string? Notes { get; set; }
        }
    }
}
