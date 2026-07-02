using MediatR;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs.Product;
using NextErp.Application.DTOs.ProductVariation;

namespace NextErp.Application.Commands
{
    // Create Product
    public record CreateProductCommand(
        string Title,
        string? Code,
        int? ParentId,
        int CategoryId,
        decimal Price,
        decimal InitialStock,
        bool IsActive = true,
        string? ImageUrl = null,
        IReadOnlyList<GalleryResolvedSlot>? ImageGallery = null,
        string? Description = null,
        string? Color = null,
        string? Warranty = null,
        int? UnitOfMeasureId = null
    ) : IRequest<int>, ITransactionalRequest; // Returns Id of created product

    // Update Product
    public record UpdateProductCommand(
        int Id,
        string Title,
        string? Code,
        int? ParentId,
        int CategoryId,
        decimal Price,
        bool IsActive = true,
        string? ImageUrl = null,
        IReadOnlyList<GalleryResolvedSlot>? ImageGallery = null,
        IReadOnlyList<ProductImageThumbnailUpdateRequest>? ImageThumbnailUpdates = null,
        string? Description = null,
        string? Color = null,
        string? Warranty = null,
        int? UnitOfMeasureId = null
    ) : IRequest<Unit>, ITransactionalRequest; // No return

    // Update Product with Variations
    public record UpdateProductWithVariationsCommand(
        int Id,
        string Title,
        string? Code,
        int? ParentId,
        int CategoryId,
        decimal Price,
        bool IsActive,
        string? ImageUrl,
        IReadOnlyList<GalleryResolvedSlot>? ImageGallery,
        IReadOnlyList<ProductImageThumbnailUpdateRequest>? ImageThumbnailUpdates,
        string? Description,
        string? Color,
        string? Warranty,
        List<VariationOptionRequest> VariationOptions,
        List<ProductVariantRequest> ProductVariants,
        int? UnitOfMeasureId = null
    ) : IRequest<Unit>, ITransactionalRequest; // No return
}
