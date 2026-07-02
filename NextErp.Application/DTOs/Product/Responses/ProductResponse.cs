using NextErp.Application.DTOs.Category;
using NextErp.Application.DTOs.ProductVariation;

namespace NextErp.Application.DTOs.Product;

public sealed record ProductResponse : ProductResponseBase
{
    public ProductMetadataRequest Metadata { get; set; } = new();
    public CategoryResponse? Category { get; set; }
    public bool HasVariations { get; set; }
    public List<VariationOptionResponse>? VariationOptions { get; set; }
    public List<ProductVariantResponse>? ProductVariants { get; set; }
    public List<ProductImageItemResponse>? Images { get; set; }

    public decimal? TotalAvailableQuantity { get; set; }

    public bool? HasLowStock { get; set; }
}
