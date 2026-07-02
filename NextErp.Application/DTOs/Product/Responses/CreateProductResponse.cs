namespace NextErp.Application.DTOs.Product;

public sealed record CreateProductResponse : ProductResponseBase
{
    public ProductMetadataRequest Metadata { get; set; } = new();
}
