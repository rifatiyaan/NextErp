namespace NextErp.Application.DTOs.Category;

public abstract record CategoryRequestBase
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public CategoryMetadataRequest Metadata { get; set; } = new();
    public List<CategoryAssetRequest> Assets { get; set; } = new();
}
