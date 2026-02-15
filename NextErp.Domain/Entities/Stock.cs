namespace NextErp.Domain.Entities
{
    public class Stock : IEntity<int>
    {
        public int Id { get; set; } // Same as ProductId (one-to-one). For multi-warehouse: change to Guid and remove unique constraint
        public string Title { get; set; } = "Stock"; // Required by IEntity
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public decimal AvailableQuantity { get; set; }
        
        public byte[] RowVersion { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; } // Can be used for warehouse/location when multi-warehouse is added
        // Future: Add WarehouseId property when implementing multi-warehouse feature
    }
}
