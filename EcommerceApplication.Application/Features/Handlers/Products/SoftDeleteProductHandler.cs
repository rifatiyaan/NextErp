using MediatR;
using Commands = EcommerceApplicationWeb.Application.Features.Products.Commands;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Features.Handlers.Product
{
    public class SoftDeleteProductHandler : IRequestHandler<Commands.SoftDeleteProductCommand, Unit>
    {
        private readonly Repositories.IProductRepository _productRepo;

        public SoftDeleteProductHandler(Repositories.IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<Unit> Handle(Commands.SoftDeleteProductCommand request, CancellationToken cancellationToken)
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
