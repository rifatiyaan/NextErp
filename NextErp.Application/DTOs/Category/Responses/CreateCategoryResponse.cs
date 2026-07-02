namespace NextErp.Application.DTOs.Category;

public sealed record CreateCategoryResponse : CategoryResponseBase
{
    public CategoryMetadataRequest Metadata { get; set; } = new();
}
