namespace NextErp.Domain.Entities
{
    public class PurchaseOrder
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public Guid SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // Draft, Received, Paid

        public ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; }
    }

}
