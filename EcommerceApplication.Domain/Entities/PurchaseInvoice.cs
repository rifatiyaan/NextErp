namespace EcommerceApplicationWeb.Domain.Entities
{
    public class PurchaseInvoice : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public Guid TenantId { get; set; }

        public Guid SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        public PurchaseInvoiceMetadata Metadata { get; set; } = new PurchaseInvoiceMetadata();

        public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public class PurchaseInvoiceMetadata
        {
            public string? ReferenceNo { get; set; }
            public string? PaymentStatus { get; set; }
            public string? Notes { get; set; }
        }
    }

}
