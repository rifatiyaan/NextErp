namespace NextErp.Domain.Entities
{
    public class Tenant : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? DatabaseConnectionString { get; set; }
        public TenantMetadata Metadata { get; set; } = new TenantMetadata();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();

        public class TenantMetadata
        {
            public string? AdminEmail { get; set; }
            public string? SubscriptionPlan { get; set; }
            public DateTime? SubscriptionExpiry { get; set; }
        }
    }
}
