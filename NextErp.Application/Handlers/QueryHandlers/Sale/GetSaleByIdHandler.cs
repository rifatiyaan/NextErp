using NextErp.Application.Queries;
using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Sale
{
    public class GetSaleByIdHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetSaleByIdQuery, Entities.Sale?>
    {
        public async Task<Entities.Sale?> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
        {
            return await unitOfWork.SaleRepository.GetByIdWithDetailsAsync(request.Id);
        }
    }
}
