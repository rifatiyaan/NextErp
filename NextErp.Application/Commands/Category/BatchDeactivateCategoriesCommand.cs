using MediatR;
using NextErp.Application.Common.Interfaces;

// Kept under top-level Commands (mirrors CategoryCommands.cs / ProductCommands.cs);
// avoids potential namespace shadowing of the DTOs.Category type used elsewhere.
namespace NextErp.Application.Commands
{
    /// <summary>
    /// Soft-deactivates a list of Category rows in a single transaction by setting
    /// IsActive=false. Returns the number of rows actually flipped (rows already
    /// inactive are silently skipped, mirroring the per-row update behaviour).
    /// </summary>
    /// <remarks>
    /// No <see cref="NextErp.Application.Common.Attributes.RequiresPermissionAttribute"/>
    /// is attached: the existing single-row Category deactivate path (UpdateCategoryCommand
    /// with IsActive=false) carries no RequiresPermission attribute either, and the brief
    /// requires that we use what exists rather than invent a new key.
    /// </remarks>
    public record BatchDeactivateCategoriesCommand(IReadOnlyList<int> Ids)
        : IRequest<int>, ITransactionalRequest;
}
