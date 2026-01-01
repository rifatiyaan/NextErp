namespace NextErp.Domain.Entities
{
    /// <summary>
    /// Purchase item (detail) entity - line items for a purchase
    /// </summary>
    public class PurchaseItem : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        
        public Guid PurchaseId { get; set; }
        public Purchase Purchase { get; set; } = null!;
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Total => Quantity * UnitCost;
        
        public DateTime CreatedAt { get; set; }
        
        public Guid TenantId { get; set; }
    }
}
