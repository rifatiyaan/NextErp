namespace NextErp.Domain.Entities
{
    /// <summary>
    /// Stock entity - one row per product for inventory tracking
    /// Uses optimistic concurrency via RowVersion
    /// </summary>
    public class Stock : IEntity<int>
    {
        public int Id { get; set; } // Same as ProductId
        public string Title { get; set; } = "Stock"; // Required by IEntity
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public decimal AvailableQuantity { get; set; }
        
        /// <summary>
        /// Concurrency token for optimistic locking
        /// </summary>
        public byte[] RowVersion { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }
    }
}
