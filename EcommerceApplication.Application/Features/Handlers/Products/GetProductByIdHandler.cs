using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Queries = EcommerceApplicationWeb.Application.Features.Products.Queries;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Features.Handlers.Product
{
    public class GetProductByIdHandler : IRequestHandler<Queries.GetProductByIdQuery, Entities.Product?>
    {
        private readonly Repositories.IProductRepository _productRepo;

        public GetProductByIdHandler(Repositories.IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<Entities.Product?> Handle(Queries.GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            return await _productRepo.Query()
                .Include(p => p.Category)
                .Include(p => p.Children)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.IsActive, cancellationToken);
        }
    }
}
