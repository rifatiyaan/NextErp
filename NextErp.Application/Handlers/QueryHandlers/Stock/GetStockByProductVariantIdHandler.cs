using NextErp.Application;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Stock
{
    public class GetStockByProductVariantIdHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetStockByProductVariantIdQuery, Entities.Stock?>
    {
        public Task<Entities.Stock?> Handle(GetStockByProductVariantIdQuery request, CancellationToken cancellationToken)
        {
            return unitOfWork.StockRepository.Query()
                .AsNoTracking()
                .Include(s => s.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                        .ThenInclude(p => p!.UnitOfMeasure)
                .FirstOrDefaultAsync(s => s.ProductVariantId == request.ProductVariantId, cancellationToken);
        }
    }
}
