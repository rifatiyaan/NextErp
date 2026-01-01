using NextErp.Application.Queries;
using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Customer
{
    public class GetCustomerByIdHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetCustomerByIdQuery, Entities.Customer?>
    {
        public async Task<Entities.Customer?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            return await unitOfWork.CustomerRepository.GetByIdAsync(request.Id);
        }
    }
}
