using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Supplier
{
    public class CreateSupplierHandler(
        IApplicationUnitOfWork unitOfWork,
        IMapper mapper)
        : IRequestHandler<CreateSupplierCommand, int>
    {
        public async Task<int> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
        {
            var supplier = mapper.Map<Entities.Supplier>(request);
            supplier.CreatedAt = DateTime.UtcNow;

            await unitOfWork.SupplierRepository.AddAsync(supplier);
            await unitOfWork.SaveAsync();
            return supplier.Id;
        }
    }
}
