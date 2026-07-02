namespace NextErp.Application.DTOs.Category;

public sealed record UpdateCategoryBulkResponse
{
    public List<UpdateCategoryResponse> Categories { get; set; } = new();
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
