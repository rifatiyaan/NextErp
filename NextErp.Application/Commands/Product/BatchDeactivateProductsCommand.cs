using MediatR;
using NextErp.Application.Common.Interfaces;

// Note: kept under top-level Commands (no .Product subnamespace) so that a sibling
// namespace named `Product` is not introduced. ProductCommands.cs / ProductVariationCommands.cs
// import `NextErp.Application.DTOs.Product` and reference its types (GalleryResolvedSlot,
// ProductImageThumbnailUpdateRequest, ...) unqualified; a `Commands.Product` namespace
// would shadow that import.
namespace NextErp.Application.Commands
{
    /// <summary>
    /// Soft-deactivates a list of Product rows by setting IsActive=false.
    /// Returns the number of rows actually flipped (rows already inactive are skipped).
    /// </summary>
    /// <remarks>
    /// Permission key intentionally omitted — the existing single-row deactivate path
    /// (UpdateProductCommand with IsActive=false) carries no RequiresPermission attribute.
    /// </remarks>
    public record BatchDeactivateProductsCommand(IReadOnlyList<int> Ids)
        : IRequest<int>, ITransactionalRequest;
}
