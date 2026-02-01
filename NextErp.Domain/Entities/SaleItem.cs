namespace NextErp.Domain.Entities
{
    public class SaleItem : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        
        public Guid SaleId { get; set; }
        public Sale Sale { get; set; } = null!;
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
        
        public DateTime CreatedAt { get; set; }
        
        public Guid TenantId { get; set; }
    }
}
