using NextErp.Application.Queries;
using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Supplier
{
    public class GetSupplierByIdHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetSupplierByIdQuery, Entities.Supplier?>
    {
        public async Task<Entities.Supplier?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
        {
            return await unitOfWork.SupplierRepository.GetByIdAsync(request.Id);
        }
    }
}
