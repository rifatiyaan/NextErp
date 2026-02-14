using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Customer
{
    public class GetCustomerByIdHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<GetCustomerByIdQuery, Entities.Customer?>
    {
        public async Task<Entities.Customer?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            return await unitOfWork.CustomerRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        }
    }
}
