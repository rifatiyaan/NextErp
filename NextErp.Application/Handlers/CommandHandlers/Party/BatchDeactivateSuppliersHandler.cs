using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Party;

/// <summary>
/// Soft-deactivates active Party rows of type <see cref="PartyType.Supplier"/> whose
/// ids are in the request list. Filtering on PartyType keeps a "deactivate suppliers"
/// action from touching a Customer row that happens to share an id namespace.
/// </summary>
public sealed class BatchDeactivateSuppliersHandler(IApplicationDbContext dbContext)
    : IRequestHandler<BatchDeactivateSuppliersCommand, int>
{
    public async Task<int> Handle(
        BatchDeactivateSuppliersCommand request,
        CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.Parties
            .Where(p => request.Ids.Contains(p.Id)
                        && p.PartyType == PartyType.Supplier
                        && p.IsActive)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.IsActive = false;
            entity.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return entities.Count;
    }
}
