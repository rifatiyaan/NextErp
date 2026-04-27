using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Sale
{
    public class GetSaleByIdHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetSaleByIdQuery, Entities.Sale?>
    {
        public async Task<Entities.Sale?> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken = default)
        {
            // Inlined from former ISaleRepository.GetByIdWithDetailsAsync.
            return await dbContext.Sales
                .AsNoTracking()
                .Include(s => s.Party)
                .Include(s => s.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.Category)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        }
    }
}
