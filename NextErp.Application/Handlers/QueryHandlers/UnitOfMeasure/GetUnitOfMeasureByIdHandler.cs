using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;

namespace NextErp.Application.Handlers.QueryHandlers.UnitOfMeasure;

public class GetUnitOfMeasureByIdHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetUnitOfMeasureByIdQuery, DTOs.UnitOfMeasure.Response.Single?>
{
    public async Task<DTOs.UnitOfMeasure.Response.Single?> Handle(
        GetUnitOfMeasureByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.UnitOfMeasures
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (entity == null) return null;

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
