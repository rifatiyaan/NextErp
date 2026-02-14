using MediatR;

namespace NextErp.Application.Commands
{
    // Create
    public record CreateCategoryCommand(
        string Title,
        string? Description,
        int? ParentId,
        List<CategoryAsset>? Assets = null
    ) : IRequest<int>; // Returns Id of created category

    // Update
    public record UpdateCategoryCommand(
        int Id,
        string Title,
        string? Description,
        int? ParentId,
        List<CategoryAsset>? Assets = null
    ) : IRequest<Unit>; // No return

    public record CategoryAsset(
        string Filename,
        string Url,
        string Type = "image",
        long? Size = null,
        DateTime UploadedAt = default
    );

    // Soft Delete
    public record SoftDeleteCategoryCommand(int Id) : IRequest<Unit>; // No return
}
