using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Purchase
{
    public class GetPurchaseByIdHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetPurchaseByIdQuery, Entities.Purchase?>
    {
        public async Task<Entities.Purchase?> Handle(GetPurchaseByIdQuery request, CancellationToken cancellationToken = default)
        {
            // Inlined from former IPurchaseRepository.GetByIdWithDetailsAsync.
            return await dbContext.Purchases
                .AsNoTracking()
                .Include(p => p.Party)
                .Include(p => p.Items)
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(pr => pr.Category)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        }
    }
}
