using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.UnitOfMeasure;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.UnitOfMeasure;

public class UpdateUnitOfMeasureHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateUnitOfMeasureCommand>
{
    public async Task Handle(UpdateUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.UnitOfMeasures
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"UnitOfMeasure {request.Id} not found.");

        entity.Name = request.Name;
        entity.Title = request.Name;
        entity.Abbreviation = request.Abbreviation;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
