using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands
{
    public record CreateVariationOptionCommandGlobal(string Name, int DisplayOrder, Guid TenantId, Guid? BranchId = null) : IRequest<int>;

    public record AssignVariationOptionToProductCommand(int ProductId, int VariationOptionId, int DisplayOrder = 0) : IRequest<int>;

    public record UnassignVariationOptionFromProductCommand(int ProductId, int VariationOptionId) : IRequest;

    public record CreateProductWithVariationsCommand(
        // Base product fields
        string Title,
        string Code,
        int? ParentId,
        int CategoryId,
        decimal Price, // Base price (variants can override)
        int Stock, // Base stock (variants can override)
        bool IsActive,
        string? ImageUrl,
        IReadOnlyList<Product.Request.GalleryResolvedSlot>? ImageGallery,
        string? Description,
        string? Color,
        string? Warranty,
        // Variation data
        List<ProductVariation.Request.VariationOptionDto> VariationOptions,
        List<ProductVariation.Request.ProductVariantDto> ProductVariants
    ) : IRequest<int>; // Returns Id of created product

    public record UpdateVariationOptionCommand(int Id, string Name, int DisplayOrder) : IRequest;
    
    public record DeleteVariationOptionCommand(int Id) : IRequest;

    public record CreateVariationValueCommand(int VariationOptionId, string Value, int DisplayOrder) : IRequest<int>;
    
    public record UpdateVariationValueCommand(int Id, string Value, int DisplayOrder) : IRequest;
    
    public record DeleteVariationValueCommand(int Id) : IRequest;
}

