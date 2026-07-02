namespace NextErp.Application.DTOs.Category;

public sealed record UpdateCategoryRequest : CategoryRequestBase
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
    public Microsoft.AspNetCore.Http.IFormFile[]? Images { get; set; }
}
