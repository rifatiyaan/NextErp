using NextErp.Application.Queries;
using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Stock
{
    public class GetStockByProductIdHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetStockByProductIdQuery, Entities.Stock?>
    {
        public async Task<Entities.Stock?> Handle(GetStockByProductIdQuery request, CancellationToken cancellationToken)
        {
            return await unitOfWork.StockRepository.GetByProductIdAsync(request.ProductId);
        }
    }
}
