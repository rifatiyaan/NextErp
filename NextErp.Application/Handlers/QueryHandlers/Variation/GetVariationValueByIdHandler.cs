using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Variation
{
    public class GetVariationValueByIdHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetVariationValueByIdQuery, Entities.VariationValue?>
    {
        public async Task<Entities.VariationValue?> Handle(GetVariationValueByIdQuery request, CancellationToken cancellationToken)
        {
            return await dbContext.VariationValues
                .FirstOrDefaultAsync(vv => vv.Id == request.Id, cancellationToken);
        }
    }
}

