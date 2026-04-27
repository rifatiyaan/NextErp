using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Stock
{
    public class GetStocksByProductIdHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetStocksByProductIdQuery, IReadOnlyList<Entities.Stock>>
    {
        public async Task<IReadOnlyList<Entities.Stock>> Handle(
            GetStocksByProductIdQuery request,
            CancellationToken cancellationToken = default)
        {
            return await dbContext.Stocks
                .AsNoTracking()
                .Include(s => s.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Where(s => s.ProductVariant.ProductId == request.ProductId)
                .OrderBy(s => s.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
