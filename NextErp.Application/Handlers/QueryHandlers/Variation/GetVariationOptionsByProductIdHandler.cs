using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Variation
{
    public class GetVariationOptionsByProductIdHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetVariationOptionsByProductIdQuery, List<Entities.VariationOption>>
    {
        public async Task<List<Entities.VariationOption>> Handle(GetVariationOptionsByProductIdQuery request, CancellationToken cancellationToken)
        {
            return await dbContext.VariationOptions
                .AsNoTracking()
                .Where(vo => vo.ProductId == request.ProductId)
                .Include(vo => vo.Values)
                .OrderBy(vo => vo.DisplayOrder)
                .ToListAsync(cancellationToken);
        }
    }
}

