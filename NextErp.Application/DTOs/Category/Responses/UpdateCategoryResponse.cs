namespace NextErp.Application.DTOs.Category;

public sealed record UpdateCategoryResponse : CategoryResponseBase
{
    public CategoryMetadataRequest Metadata { get; set; } = new();
}
