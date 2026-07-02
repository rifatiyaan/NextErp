namespace NextErp.Application.DTOs.Category;

public sealed record CreateCategoryRequest : CategoryRequestBase
{
    public Microsoft.AspNetCore.Http.IFormFile[]? Images { get; set; }
}
