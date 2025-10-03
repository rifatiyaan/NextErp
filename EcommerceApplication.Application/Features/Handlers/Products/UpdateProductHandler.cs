using MediatR;
using Commands = EcommerceApplicationWeb.Application.Features.Products.Commands;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Features.Handlers.Product
{
    public class UpdateProductHandler : IRequestHandler<Commands.UpdateProductCommand, Unit>
    {
        private readonly Repositories.IProductRepository _productRepo;

        public UpdateProductHandler(Repositories.IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<Unit> Handle(Commands.UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var existing = await _productRepo.GetByIdAsync(request.Id);
            if (existing != null && existing.IsActive)
            {
                existing.Title = request.Title;
                existing.Code = request.Code;
                existing.ParentId = request.ParentId;
                existing.CategoryId = request.CategoryId;
                existing.Price = request.Price;
                existing.Stock = request.Stock;
                existing.ImageUrl = request.ImageUrl;

                existing.Metadata.Description = request.Description;
                existing.Metadata.Color = request.Color;
                existing.Metadata.Warranty = request.Warranty;

                existing.UpdatedAt = DateTime.UtcNow;

                await _productRepo.EditAsync(existing);
            }

            return Unit.Value;
        }
    }
}
