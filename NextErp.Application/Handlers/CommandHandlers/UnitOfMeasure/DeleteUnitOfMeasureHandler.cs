using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.UnitOfMeasure;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.UnitOfMeasure;

public class DeleteUnitOfMeasureHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteUnitOfMeasureCommand>
{
    public async Task Handle(DeleteUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.UnitOfMeasures
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"UnitOfMeasure {request.Id} not found.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
