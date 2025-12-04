namespace NextErp.Domain.Entities
{
    public class SalesInvoice : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public Guid TenantId { get; set; }

        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        public SalesInvoiceMetadata Metadata { get; set; } = new SalesInvoiceMetadata();

        public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public class SalesInvoiceMetadata
        {
            public string? ReferenceNo { get; set; }
            public string? PaymentMethod { get; set; }
            public string? Notes { get; set; }
        }

        public enum SalesInvoiceType
        {
            Sales = 1,
            Order = 2,
            Quotation = 3,
            Return = 4,
            EcommerceOrder = 5
        }
        public enum SalesInvoiceStatus
        {
            Sold = 1,
            Returned = 2
        }
    }

}
