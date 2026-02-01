using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.QueryHandlers.Product
{
    public class GetProductByIdHandler(Repositories.IProductRepository productRepo) 
        : IRequestHandler<GetProductByIdQuery, Entities.Product?>
    {
        public async Task<Entities.Product?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            return await productRepo.Query()
                .Include(p => p.Category)
                .Include(p => p.Children)
                .Include(p => p.VariationOptions)
                    .ThenInclude(vo => vo.Values)
                .Include(p => p.ProductVariants)
                    .ThenInclude(pv => pv.VariationValues)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.IsActive, cancellationToken);
        }
    }
}
