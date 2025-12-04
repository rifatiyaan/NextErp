namespace NextErp.Domain.Entities
{
    public class Customer : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;

        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public CustomerMetadata Metadata { get; set; } = new CustomerMetadata();

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }

        public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();

        public class CustomerMetadata
        {
            public string? LoyaltyCode { get; set; }
            public string? Notes { get; set; }
            public string? NationalId { get; set; }
        }
    }

}
