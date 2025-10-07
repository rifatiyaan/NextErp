using EcommerceApplicationWeb.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.QueryHandlers.Product
{
    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Entities.Product?>
    {
        private readonly Repositories.IProductRepository _productRepo;

        public GetProductByIdHandler(Repositories.IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<Entities.Product?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            return await _productRepo.Query()
                .Include(p => p.Category)
                .Include(p => p.Children)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.IsActive, cancellationToken);
        }
    }
}
