using MediatR;
using NextErp.Application.Common.Interfaces;

// Note: kept under top-level Commands (no .Product subnamespace) so the global
// `Product.Request.*` references inside ProductCommands.cs / ProductVariationCommands.cs
// (which resolve to the DTOs.Product type via their using imports) don't get hijacked
// by a fresh sibling namespace named `Product`.
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
