using AutoMapper;
using NextErp.Application.Commands;
using MediatR;

namespace NextErp.Application.Handlers.CommandHandlers.Supplier
{
    public class UpdateSupplierHandler(
        IApplicationUnitOfWork unitOfWork,
        IMapper mapper)
        : IRequestHandler<UpdateSupplierCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
        {
            var supplier = await unitOfWork.SupplierRepository.GetByIdAsync(request.Id);
            if (supplier == null)
                throw new InvalidOperationException($"Supplier with Id {request.Id} not found.");

            mapper.Map(request, supplier);
            supplier.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.SupplierRepository.EditAsync(supplier);
            await unitOfWork.SaveAsync();
            return Unit.Value;
        }
    }
}
