using MediatR;
using NextErp.Application.Common.Interfaces;

// Kept under top-level Commands (mirrors PartyCommands.cs); avoids potential
// namespace shadowing of the DTOs.Party type used elsewhere.
namespace NextErp.Application.Commands
{
    /// <summary>
    /// Soft-deactivates a list of Customer party rows. Filters by PartyType=Customer
    /// so id reuse with Suppliers cannot accidentally deactivate the wrong row type.
    /// </summary>
    /// <remarks>
    /// Permission key intentionally omitted — the existing single-row deactivate path
    /// (UpdatePartyCommand with IsActive=false) carries no RequiresPermission attribute.
    /// </remarks>
    public record BatchDeactivateCustomersCommand(IReadOnlyList<Guid> Ids)
        : IRequest<int>, ITransactionalRequest;
}
