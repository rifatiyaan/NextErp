using NextErp.Application.DTOs.ProductVariation;

namespace NextErp.Application.DTOs.Product;

public sealed record UpdateProductRequest : ProductRequestBase
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;

    // Variation system support (optional - if null/empty, product has no variations)
    public bool HasVariations { get; set; } = false;
    public List<VariationOptionRequest>? VariationOptions { get; set; } // null = simple product
    public List<ProductVariantRequest>? ProductVariants { get; set; } // null = simple product
}
