namespace EcommerceApplicationWeb.Domain.Entities
{
    public class Branch : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Address { get; set; }
        public BranchMetadata Metadata { get; set; } = new BranchMetadata();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public class BranchMetadata
        {
            public string? Phone { get; set; }
            public string? ManagerName { get; set; }
            public string? BranchCode { get; set; }
        }
    }
}
