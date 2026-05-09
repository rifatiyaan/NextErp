using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.Product;

/// <summary>
/// Soft-deactivates active Product rows whose ids are in the request list.
/// Stock rows and product variants are intentionally left untouched — only
/// the parent product's IsActive flag is flipped, matching the per-row update path.
/// </summary>
public sealed class BatchDeactivateProductsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<BatchDeactivateProductsCommand, int>
{
    public async Task<int> Handle(
        BatchDeactivateProductsCommand request,
        CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.Products
            .Where(p => request.Ids.Contains(p.Id) && p.IsActive)
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
