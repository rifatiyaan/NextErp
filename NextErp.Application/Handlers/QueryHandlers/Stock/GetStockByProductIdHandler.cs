using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Stock
{
    public class GetStockByProductIdHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetStockByProductIdQuery, Entities.Stock?>
    {
        public async Task<Entities.Stock?> Handle(GetStockByProductIdQuery request, CancellationToken cancellationToken)
        {
            return await unitOfWork.StockRepository.Query()
                .AsNoTracking()
                .Include(s => s.Product)
                    .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);
        }
    }
}
