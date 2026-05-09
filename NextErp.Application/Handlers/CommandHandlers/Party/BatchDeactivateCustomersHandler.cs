using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Party;

/// <summary>
/// Soft-deactivates active Party rows of type <see cref="PartyType.Customer"/> whose
/// ids are in the request list. Filtering on PartyType prevents id reuse from
/// accidentally hitting a Supplier row when the operator clicks "deactivate customers".
/// </summary>
public sealed class BatchDeactivateCustomersHandler(IApplicationDbContext dbContext)
    : IRequestHandler<BatchDeactivateCustomersCommand, int>
{
    public async Task<int> Handle(
        BatchDeactivateCustomersCommand request,
        CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.Parties
            .Where(p => request.Ids.Contains(p.Id)
                        && p.PartyType == PartyType.Customer
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
