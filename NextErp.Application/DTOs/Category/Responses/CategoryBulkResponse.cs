namespace NextErp.Application.DTOs.Category;

public sealed record CategoryBulkResponse
{
    public List<CategoryResponse> Categories { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
