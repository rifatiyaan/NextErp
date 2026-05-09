using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.Category;

/// <summary>
/// Soft-deactivates active Category rows whose ids are in the request list.
/// Rows already inactive are silently skipped (returned count reflects the actual flips).
/// Branch scoping comes from the global query filter on dbContext.Categories.
/// </summary>
public sealed class BatchDeactivateCategoriesHandler(IApplicationDbContext dbContext)
    : IRequestHandler<BatchDeactivateCategoriesCommand, int>
{
    public async Task<int> Handle(
        BatchDeactivateCategoriesCommand request,
        CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.Categories
            .Where(c => request.Ids.Contains(c.Id) && c.IsActive)
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
