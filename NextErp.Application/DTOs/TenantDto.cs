namespace NextErp.Application.DTOs
{
    public class TenantRequestDto
    {
        public string Name { get; set; } = null!;
        public string? DatabaseConnectionString { get; set; }
        public TenantMetadataDto Metadata { get; set; } = new TenantMetadataDto();
        public bool IsActive { get; set; } = true;
    }

    public class TenantResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? DatabaseConnectionString { get; set; }
        public TenantMetadataDto Metadata { get; set; } = new TenantMetadataDto();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BranchResponseDto>? Branches { get; set; }
    }

    public class TenantMetadataDto
    {
        public string? AdminEmail { get; set; }
        public string? SubscriptionPlan { get; set; }
        public DateTime? SubscriptionExpiry { get; set; }
    }
}
