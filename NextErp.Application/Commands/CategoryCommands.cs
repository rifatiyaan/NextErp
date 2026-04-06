using MediatR;
using NextErp.Application.Common.Interfaces;

namespace NextErp.Application.Commands
{
    // Create
    public record CreateCategoryCommand(
        string Title,
        string? Description,
        int? ParentId,
        List<CategoryAsset>? Assets = null
    ) : IRequest<int>, ITransactionalRequest; // Returns Id of created category

    // Update
    public record UpdateCategoryCommand(
        int Id,
        string Title,
        string? Description,
        int? ParentId,
        bool IsActive = true,
        List<CategoryAsset>? Assets = null
    ) : IRequest<Unit>, ITransactionalRequest; // No return

    public record CategoryAsset(
        string Filename,
        string Url,
        string Type = "image",
        long? Size = null,
        DateTime UploadedAt = default
    );
}
