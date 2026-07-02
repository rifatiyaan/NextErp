using MediatR;
using NextErp.Application.Commands.UnitOfMeasure;
using NextErp.Application.DTOs.UnitOfMeasure;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.UnitOfMeasure;

public class CreateUnitOfMeasureHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateUnitOfMeasureCommand, UnitOfMeasureResponse>
{
    public async Task<UnitOfMeasureResponse> Handle(
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

        return new UnitOfMeasureResponse
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
