using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Variation
{
    public class GetVariationOptionByIdHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetVariationOptionByIdQuery, Entities.VariationOption?>
    {
        public async Task<Entities.VariationOption?> Handle(GetVariationOptionByIdQuery request, CancellationToken cancellationToken)
        {
            return await dbContext.VariationOptions
                .Include(vo => vo.Values)
                .FirstOrDefaultAsync(vo => vo.Id == request.Id, cancellationToken);
        }
    }
}

