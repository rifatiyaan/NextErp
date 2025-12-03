using EcommerceApplicationWeb.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.QueryHandlers.Product
{
    public class GetProductByIdHandler(Repositories.IProductRepository productRepo) 
        : IRequestHandler<GetProductByIdQuery, Entities.Product?>
    {
        public async Task<Entities.Product?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            return await productRepo.Query()
                .Include(p => p.Category)
                .Include(p => p.Children)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.IsActive, cancellationToken);
        }
    }
}
