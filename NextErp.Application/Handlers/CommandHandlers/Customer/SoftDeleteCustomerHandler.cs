using NextErp.Application.Commands;
using MediatR;

namespace NextErp.Application.Handlers.CommandHandlers.Customer
{
    public class SoftDeleteCustomerHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<SoftDeleteCustomerCommand, Unit>
    {
        public async Task<Unit> Handle(SoftDeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await unitOfWork.CustomerRepository.GetByIdAsync(request.Id);
            if (customer == null)
                throw new InvalidOperationException($"Customer with Id {request.Id} not found.");

            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.CustomerRepository.EditAsync(customer);
            await unitOfWork.SaveAsync();
            return Unit.Value;
        }
    }
}
