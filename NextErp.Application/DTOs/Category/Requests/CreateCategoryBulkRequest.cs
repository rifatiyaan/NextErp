namespace NextErp.Application.DTOs.Category;

public sealed record CreateCategoryBulkRequest
{
    public List<CreateCategoryRequest> Categories { get; set; } = new();
}
