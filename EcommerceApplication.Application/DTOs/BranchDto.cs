namespace EcommerceApplicationWeb.Application.DTOs
{
    public class BranchRequestDto
    {
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public BranchMetadataDto Metadata { get; set; } = new BranchMetadataDto();
        public bool IsActive { get; set; } = true;
    }

    public class BranchResponseDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public BranchMetadataDto Metadata { get; set; } = new BranchMetadataDto();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public TenantResponseDto? Tenant { get; set; }
    }

    public class BranchMetadataDto
    {
        public string? Phone { get; set; }
        public string? ManagerName { get; set; }
        public string? BranchCode { get; set; }
        public string? Email { get; set; }
    }
}
