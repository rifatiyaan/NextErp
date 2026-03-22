using NextErp.Application;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Stock
{
    public class GetStocksByProductIdHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetStocksByProductIdQuery, IReadOnlyList<Entities.Stock>>
    {
        public async Task<IReadOnlyList<Entities.Stock>> Handle(
            GetStocksByProductIdQuery request,
            CancellationToken cancellationToken)
        {
            return await unitOfWork.StockRepository.Query()
                .AsNoTracking()
                .Include(s => s.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Where(s => s.ProductVariant.ProductId == request.ProductId)
                .OrderBy(s => s.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
