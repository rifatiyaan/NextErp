namespace NextErp.Domain.Entities
{
    public class Branch : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public Guid TenantId { get; set; }
        public string? Address { get; set; }
        public BranchMetadata Metadata { get; set; } = new BranchMetadata();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        public class BranchMetadata
        {
            public string? Phone { get; set; }
            public string? ManagerName { get; set; }
            public string? BranchCode { get; set; }
        }
    }
}
