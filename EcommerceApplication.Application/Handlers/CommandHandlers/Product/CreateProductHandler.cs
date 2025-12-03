using EcommerceApplicationWeb.Application.Commands;
using MediatR;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.CommandHandlers.Product
{
    public class CreateProductHandler(Repositories.IProductRepository productRepo) 
        : IRequestHandler<CreateProductCommand, int>
    {
        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
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

            await productRepo.AddAsync(product);
            return product.Id;
        }
    }
}
