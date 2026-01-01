namespace NextErp.Domain.Entities
{
    /// <summary>
    /// Purchase master entity - represents a purchase transaction from supplier
    /// </summary>
    public class Purchase : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string PurchaseNumber { get; set; } = null!;
        
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;
        
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        
        public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
        
        public PurchaseMetadata Metadata { get; set; } = new PurchaseMetadata();
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }
        
        public class PurchaseMetadata
        {
            public string? ReferenceNo { get; set; }
            public string? Notes { get; set; }
        }
    }
}
