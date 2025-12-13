using NextErp.Application.Commands;
using MediatR;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class SoftDeleteProductHandler(IApplicationUnitOfWork unitOfWork) 
        : IRequestHandler<SoftDeleteProductCommand, Unit>
    {
        public async Task<Unit> Handle(SoftDeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await unitOfWork.ProductRepository.GetByIdAsync(request.Id);
            if (product != null && product.IsActive)
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await unitOfWork.ProductRepository.EditAsync(product);
                await unitOfWork.SaveAsync();
            }

            return Unit.Value;
        }
    }
}
