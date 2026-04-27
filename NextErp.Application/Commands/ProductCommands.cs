using MediatR;
using NextErp.Application.Common.Interfaces;
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
        decimal InitialStock,
        bool IsActive = true,
        string? ImageUrl = null,
        IReadOnlyList<Product.Request.GalleryResolvedSlot>? ImageGallery = null,
        string? Description = null,
        string? Color = null,
        string? Warranty = null,
        int? UnitOfMeasureId = null
    ) : IRequest<int>, ITransactionalRequest; // Returns Id of created product

    // Update Product
    public record UpdateProductCommand(
        int Id,
        string Title,
        string Code,
        int? ParentId,
        int CategoryId,
        decimal Price,
        bool IsActive = true,
        string? ImageUrl = null,
        IReadOnlyList<Product.Request.GalleryResolvedSlot>? ImageGallery = null,
        IReadOnlyList<Product.Request.ProductImageThumbnailUpdate>? ImageThumbnailUpdates = null,
        string? Description = null,
        string? Color = null,
        string? Warranty = null,
        int? UnitOfMeasureId = null
    ) : IRequest<Unit>, ITransactionalRequest; // No return

    // Update Product with Variations
    public record UpdateProductWithVariationsCommand(
        int Id,
        string Title,
        string Code,
        int? ParentId,
        int CategoryId,
        decimal Price,
        bool IsActive,
        string? ImageUrl,
        IReadOnlyList<Product.Request.GalleryResolvedSlot>? ImageGallery,
        IReadOnlyList<Product.Request.ProductImageThumbnailUpdate>? ImageThumbnailUpdates,
        string? Description,
        string? Color,
        string? Warranty,
        List<ProductVariation.Request.VariationOptionDto> VariationOptions,
        List<ProductVariation.Request.ProductVariantDto> ProductVariants,
        int? UnitOfMeasureId = null
    ) : IRequest<Unit>, ITransactionalRequest; // No return
}
