namespace NextErp.Application.DTOs.Category;

public abstract record CategoryResponseBase
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public List<CategoryAssetRequest> Assets { get; set; } = new();
}
