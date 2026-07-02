using NextErp.Application.DTOs.ProductVariation;

namespace NextErp.Application.DTOs.Product;

public sealed record CreateProductRequest : ProductRequestBase
{
    public bool IsActive { get; set; } = true;

    // Seed quantity for first Stock row; no longer persisted on Product itself.
    public decimal InitialStock { get; set; }

    // Variation system support (optional - if null/empty, product has no variations)
    public bool HasVariations { get; set; } = false;
    public List<VariationOptionRequest>? VariationOptions { get; set; } // null = simple product
    public List<ProductVariantRequest>? ProductVariants { get; set; } // null = simple product
}
