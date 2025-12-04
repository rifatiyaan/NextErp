using NextErp.Application.Commands;
using MediatR;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class SoftDeleteProductHandler(Repositories.IProductRepository productRepo) 
        : IRequestHandler<SoftDeleteProductCommand, Unit>
    {
        public async Task<Unit> Handle(SoftDeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await productRepo.GetByIdAsync(request.Id);
            if (product != null && product.IsActive)
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await productRepo.EditAsync(product);
            }

            return Unit.Value;
        }
    }
}
