using MediatR;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs.Product;
using NextErp.Application.DTOs.ProductVariation;

namespace NextErp.Application.Commands
{
    public record CreateVariationOptionCommandGlobal(string Name, int DisplayOrder, Guid TenantId, Guid? BranchId = null) : IRequest<int>, ITransactionalRequest;

    public record AssignVariationOptionToProductCommand(int ProductId, int VariationOptionId, int DisplayOrder = 0) : IRequest<int>, ITransactionalRequest;

    public record UnassignVariationOptionFromProductCommand(int ProductId, int VariationOptionId) : IRequest, ITransactionalRequest;

    public record CreateProductWithVariationsCommand(
        // Base product fields
        string Title,
        string? Code,
        int? ParentId,
        int CategoryId,
        decimal Price, // Base price (variants can override)
        decimal InitialStock, // Seed quantity for default variant (variants override per-variant)
        bool IsActive,
        string? ImageUrl,
        IReadOnlyList<GalleryResolvedSlot>? ImageGallery,
        string? Description,
        string? Color,
        string? Warranty,
        // Variation data
        List<VariationOptionRequest> VariationOptions,
        List<ProductVariantRequest> ProductVariants,
        int? UnitOfMeasureId = null
    ) : IRequest<int>, ITransactionalRequest; // Returns Id of created product

    public record UpdateVariationOptionCommand(int Id, string Name, int DisplayOrder) : IRequest, ITransactionalRequest;

    public record DeleteVariationOptionCommand(int Id) : IRequest, ITransactionalRequest;

    public record CreateVariationValueCommand(int VariationOptionId, string Value, int DisplayOrder) : IRequest<int>, ITransactionalRequest;

    public record UpdateVariationValueCommand(int Id, string Value, int DisplayOrder) : IRequest, ITransactionalRequest;

    public record DeleteVariationValueCommand(int Id) : IRequest, ITransactionalRequest;
}

