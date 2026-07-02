using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs.UnitOfMeasure;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;

namespace NextErp.Application.Handlers.QueryHandlers.UnitOfMeasure;

public class GetAllUnitOfMeasuresHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAllUnitOfMeasuresQuery, IReadOnlyList<UnitOfMeasureResponse>>
{
    public async Task<IReadOnlyList<UnitOfMeasureResponse>> Handle(
        GetAllUnitOfMeasuresQuery request, CancellationToken cancellationToken = default)
    {
        return await dbContext.UnitOfMeasures
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new UnitOfMeasureResponse
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
