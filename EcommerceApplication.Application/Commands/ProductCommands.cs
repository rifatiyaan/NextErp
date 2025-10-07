using MediatR;

namespace EcommerceApplicationWeb.Application.Commands
{
    // Create Product
    public record CreateProductCommand(
        string Title,
        string Code,
        int? ParentId,
        int CategoryId,
        decimal Price,
        int Stock,
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
        string? ImageUrl = null,
        string? Description = null,
        string? Color = null,
        string? Warranty = null
    ) : IRequest<Unit>; // No return

    // Soft Delete Product
    public record SoftDeleteProductCommand(int Id) : IRequest<Unit>; // No return
}
