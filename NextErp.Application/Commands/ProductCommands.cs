using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands
{
    // Create Product
    public record CreateProductCommand(
        string Title,
        string Code,
        int? ParentId,
        int CategoryId,
        decimal Price,
        int Stock,
        bool IsActive = true,
        string? ImageUrl = null,
        string? Description = null,
        string? Color = null,
        string? Warranty = null
    ) : IRequest<int>; // Returns Id of created product

    // Update Product
    public record UpdateProductCommand(
        int Id,
        string Title,
        string Code,
        int? ParentId,
        int CategoryId,
        decimal Price,
        int Stock,
        bool IsActive = true,
        string? ImageUrl = null,
        string? Description = null,
        string? Color = null,
        string? Warranty = null
    ) : IRequest<Unit>; // No return

    // Update Product with Variations
    public record UpdateProductWithVariationsCommand(
        int Id,
        string Title,
        string Code,
        int? ParentId,
        int CategoryId,
        decimal Price,
        int Stock,
        bool IsActive,
        string? ImageUrl,
        string? Description,
        string? Color,
        string? Warranty,
        List<ProductVariation.Request.VariationOptionDto> VariationOptions,
        List<ProductVariation.Request.ProductVariantDto> ProductVariants
    ) : IRequest<Unit>; // No return

    // Soft Delete Product
    public record SoftDeleteProductCommand(int Id) : IRequest<Unit>; // No return
}
