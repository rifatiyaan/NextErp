namespace NextErp.Application.DTOs.Product;

public sealed record UpdateProductResponse : ProductResponseBase
{
    public ProductMetadataRequest Metadata { get; set; } = new();
}
