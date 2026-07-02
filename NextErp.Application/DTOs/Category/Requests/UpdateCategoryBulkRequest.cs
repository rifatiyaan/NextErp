namespace NextErp.Application.DTOs.Category;

public sealed record UpdateCategoryBulkRequest
{
    public List<UpdateCategoryRequest> Categories { get; set; } = new();
}
