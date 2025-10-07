using EcommerceApplicationWeb.Application.Commands;
using MediatR;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.CommandHandlers.Product
{
    public class SoftDeleteProductHandler : IRequestHandler<SoftDeleteProductCommand, Unit>
    {
        private readonly Repositories.IProductRepository _productRepo;

        public SoftDeleteProductHandler(Repositories.IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<Unit> Handle(SoftDeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepo.GetByIdAsync(request.Id);
            if (product != null && product.IsActive)
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepo.EditAsync(product);
            }

            return Unit.Value;
        }
    }
}
