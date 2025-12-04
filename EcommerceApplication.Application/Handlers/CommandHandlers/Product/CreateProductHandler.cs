using AutoMapper;
using EcommerceApplicationWeb.Application.Commands;
using MediatR;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.CommandHandlers.Product
{
    public class CreateProductHandler(
        Repositories.IProductRepository productRepo,
        IMapper mapper) 
        : IRequestHandler<CreateProductCommand, int>
    {
        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = mapper.Map<Entities.Product>(request);
            product.IsActive = true;
            product.CreatedAt = DateTime.UtcNow;

            await productRepo.AddAsync(product);
            return product.Id;
        }
    }
}
