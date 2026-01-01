using AutoMapper;
using NextErp.Application.Commands;
using MediatR;

namespace NextErp.Application.Handlers.CommandHandlers.Customer
{
    public class UpdateCustomerHandler(
        IApplicationUnitOfWork unitOfWork,
        IMapper mapper)
        : IRequestHandler<UpdateCustomerCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await unitOfWork.CustomerRepository.GetByIdAsync(request.Id);
            if (customer == null)
                throw new InvalidOperationException($"Customer with Id {request.Id} not found.");

            mapper.Map(request, customer);
            customer.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.CustomerRepository.EditAsync(customer);
            await unitOfWork.SaveAsync();
            return Unit.Value;
        }
    }
}
