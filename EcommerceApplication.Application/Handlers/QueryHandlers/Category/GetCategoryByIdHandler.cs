using EcommerceApplicationWeb.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.QueryHandlers.Category
{
    public class GetCategoryByIdHandler(Repositories.ICategoryRepository categoryRepo)
        : IRequestHandler<GetCategoryByIdQuery, Entities.Category?>
    {
        public async Task<Entities.Category?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            return await categoryRepo.Query()
                .Include(c => c.Products)
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.IsActive, cancellationToken);
        }
    }
}
