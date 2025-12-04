namespace NextErp.Domain.Entities
{
    public class PurchaseOrderLine
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }

}
