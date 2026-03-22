namespace NextErp.Domain.Entities
{
    /// <summary>1:1 ledger row per sellable SKU. <see cref="Id"/> equals <see cref="ProductVariant"/> id.</summary>
    public class Stock : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = "Stock";

        public ProductVariant ProductVariant { get; set; } = null!;
        
        public decimal AvailableQuantity { get; set; }
        
        public byte[] RowVersion { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; } // Can be used for warehouse/location when multi-warehouse is added
        // Future: Add WarehouseId property when implementing multi-warehouse feature
    }
}
