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
            var product = await dbContext.Products
                .AsNoTracking()
                .Include(p => p.ProductVariationOptions)
                    .ThenInclude(pvo => pvo.VariationOption)
                    .ThenInclude(vo => vo.Values.OrderBy(v => v.DisplayOrder))
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
                return new List<Entities.VariationOption>();

            return product.ProductVariationOptions
                .OrderBy(pvo => pvo.DisplayOrder)
                .Select(pvo => pvo.VariationOption)
                .ToList();
        }
    }
}
