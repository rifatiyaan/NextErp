namespace NextErp.Domain.Entities
{
    public class Sale : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string SaleNumber { get; set; } = null!;

        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; } = 0;
        public decimal Tax { get; set; } = 0;
        public decimal FinalAmount { get; set; }

        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();

        public SaleMetadata Metadata { get; set; } = new SaleMetadata();

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }

        public class SaleMetadata
        {
            public string? ReferenceNo { get; set; }
            public string? PaymentMethod { get; set; }
            public string? Notes { get; set; }
        }
    }
}
