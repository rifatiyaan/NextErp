using NextErp.Application.Commands;
using MediatR;

namespace NextErp.Application.Handlers.CommandHandlers.Supplier
{
    public class SoftDeleteSupplierHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<SoftDeleteSupplierCommand, Unit>
    {
        public async Task<Unit> Handle(SoftDeleteSupplierCommand request, CancellationToken cancellationToken)
        {
            var supplier = await unitOfWork.SupplierRepository.GetByIdAsync(request.Id);
            if (supplier == null)
                throw new InvalidOperationException($"Supplier with Id {request.Id} not found.");

            supplier.IsActive = false;
            supplier.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.SupplierRepository.EditAsync(supplier);
            await unitOfWork.SaveAsync();
            return Unit.Value;
        }
    }
}
