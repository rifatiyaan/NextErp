using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Stock
{
    public class GetStockByProductVariantIdHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetStockByProductVariantIdQuery, Entities.Stock?>
    {
        public Task<Entities.Stock?> Handle(GetStockByProductVariantIdQuery request, CancellationToken cancellationToken = default)
        {
            return dbContext.Stocks
                .AsNoTracking()
                .Include(s => s.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                        .ThenInclude(p => p!.UnitOfMeasure)
                .FirstOrDefaultAsync(s => s.ProductVariantId == request.ProductVariantId, cancellationToken);
        }
    }
}
