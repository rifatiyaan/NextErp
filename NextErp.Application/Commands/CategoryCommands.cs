using MediatR;

namespace NextErp.Application.Commands
{
    // Create
    public record CreateCategoryCommand(
        string Title,
        string? Description,
        int? ParentId
    ) : IRequest<int>; // Returns Id of created category

    // Update
    public record UpdateCategoryCommand(
        int Id,
        string Title,
        string? Description,
        int? ParentId
    ) : IRequest<Unit>; // No return

    // Soft Delete
    public record SoftDeleteCategoryCommand(int Id) : IRequest<Unit>; // No return
}
