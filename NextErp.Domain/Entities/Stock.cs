using NextErp.Domain.Common;

namespace NextErp.Domain.Entities
{
    /// <summary>Branch-scoped stock row per sellable SKU.</summary>
    [BranchScoped]
    public class Stock : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "Stock";

        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;
        
        public decimal AvailableQuantity { get; set; }
        
        public byte[] RowVersion { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }
    }
}
