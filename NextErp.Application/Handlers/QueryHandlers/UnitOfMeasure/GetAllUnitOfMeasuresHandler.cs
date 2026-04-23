using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;

namespace NextErp.Application.Handlers.QueryHandlers.UnitOfMeasure;

public class GetAllUnitOfMeasuresHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAllUnitOfMeasuresQuery, IReadOnlyList<DTOs.UnitOfMeasure.Response.Single>>
{
    public async Task<IReadOnlyList<DTOs.UnitOfMeasure.Response.Single>> Handle(
        GetAllUnitOfMeasuresQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.UnitOfMeasures
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new DTOs.UnitOfMeasure.Response.Single
            {
                Id = u.Id,
                Title = u.Title,
                Name = u.Name,
                Abbreviation = u.Abbreviation,
                Category = u.Category,
                IsSystem = u.IsSystem,
                IsActive = u.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
