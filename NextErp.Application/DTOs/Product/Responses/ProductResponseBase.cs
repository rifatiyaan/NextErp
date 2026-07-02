namespace NextErp.Application.DTOs.Product;

public abstract record ProductResponseBase
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Code { get; set; } = null!;
    public decimal Price { get; set; }
    public int? CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentId { get; set; }
    public int? UnitOfMeasureId { get; set; }
    public string? UnitAbbreviation { get; set; }
    public string? UnitTitle { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
}
