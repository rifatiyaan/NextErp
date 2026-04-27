using MediatR;
using NextErp.Application.Commands.UnitOfMeasure;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.UnitOfMeasure;

public class CreateUnitOfMeasureHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateUnitOfMeasureCommand, DTOs.UnitOfMeasure.Response.Single>
{
    public async Task<DTOs.UnitOfMeasure.Response.Single> Handle(
        CreateUnitOfMeasureCommand request, CancellationToken cancellationToken = default)
    {
        var entity = new Domain.Entities.UnitOfMeasure
        {
            Name = request.Name,
            Title = request.Name,
            Abbreviation = request.Abbreviation,
            Category = request.Category,
            IsSystem = request.IsSystem,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.UnitOfMeasures.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DTOs.UnitOfMeasure.Response.Single
        {
            Id = entity.Id,
            Title = entity.Title,
            Name = entity.Name,
            Abbreviation = entity.Abbreviation,
            Category = entity.Category,
            IsSystem = entity.IsSystem,
            IsActive = entity.IsActive
        };
    }
}
