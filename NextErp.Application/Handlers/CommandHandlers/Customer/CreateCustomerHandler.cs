using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Customer
{
    public class CreateCustomerHandler(
        IApplicationUnitOfWork unitOfWork,
        IMapper mapper)
        : IRequestHandler<CreateCustomerCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = mapper.Map<Entities.Customer>(request);
            customer.Id = Guid.NewGuid();
            customer.CreatedAt = DateTime.UtcNow;

            await unitOfWork.CustomerRepository.AddAsync(customer);
            await unitOfWork.SaveAsync();
            return customer.Id;
        }
    }
}
