using MediatR;
using Commands = EcommerceApplicationWeb.Application.Features.Products.Commands;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Features.Handlers.Product
{
    public class CreateProductHandler : IRequestHandler<Commands.CreateProductCommand, int>
    {
        private readonly Repositories.IProductRepository _productRepo;

        public CreateProductHandler(Repositories.IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<int> Handle(Commands.CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Entities.Product
            {
                Title = request.Title,
                Code = request.Code,
                ParentId = request.ParentId,
                CategoryId = request.CategoryId,
                Price = request.Price,
                Stock = request.Stock,
                ImageUrl = request.ImageUrl,
                Metadata = new Entities.Product.ProductMetadataClass
                {
                    Description = request.Description,
                    Color = request.Color,
                    Warranty = request.Warranty
                },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepo.AddAsync(product);
            return product.Id;
        }
    }
}
