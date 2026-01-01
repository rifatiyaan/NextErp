using NextErp.Application.Queries;
using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Purchase
{
    public class GetPurchaseByIdHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetPurchaseByIdQuery, Entities.Purchase?>
    {
        public async Task<Entities.Purchase?> Handle(GetPurchaseByIdQuery request, CancellationToken cancellationToken)
        {
            return await unitOfWork.PurchaseRepository.GetByIdWithDetailsAsync(request.Id);
        }
    }
}
