using NextErp.Application.DTOs.Product;

namespace NextErp.Application.DTOs.Category;

public sealed record CategoryResponse : CategoryResponseBase
{
    public CategoryMetadataRequest Metadata { get; set; } = new();
    public List<ProductResponse>? Products { get; set; }
}
