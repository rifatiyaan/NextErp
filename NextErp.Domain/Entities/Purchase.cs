using NextErp.Domain.Common;

namespace NextErp.Domain.Entities
{
[BranchScoped]
public class Purchase : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string PurchaseNumber { get; set; } = null!;
        
        public Guid? PartyId { get; set; }
        public Party? Party { get; set; }
        
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetTotal => TotalAmount - Discount;
        
        public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
        
        public PurchaseMetadata Metadata { get; set; } = new PurchaseMetadata();
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }
        
        public class PurchaseMetadata
        {
            public string? BatchNo { get; set; }
            public string? BillNo { get; set; }
            public string? ChallanNo { get; set; }
            public string? ReferenceNo { get; set; }
            public string? Notes { get; set; }
        }
    }
}
