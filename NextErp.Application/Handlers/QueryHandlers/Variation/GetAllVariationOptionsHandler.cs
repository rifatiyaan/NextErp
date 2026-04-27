using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;

namespace NextErp.Application.Handlers.QueryHandlers.Variation
{
    public class GetAllVariationOptionsHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetAllVariationOptionsQuery, List<Domain.Entities.VariationOption>>
    {
        public async Task<List<Domain.Entities.VariationOption>> Handle(GetAllVariationOptionsQuery request, CancellationToken cancellationToken = default)
        {
            return await dbContext.VariationOptions
                .AsNoTracking()
                .Include(vo => vo.Values.OrderBy(v => v.DisplayOrder))
                .Where(vo => vo.IsActive)
                .OrderBy(vo => vo.DisplayOrder)
                .ThenBy(vo => vo.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
