using EcommerceApplicationWeb.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.QueryHandlers.Category
{
    public class GetCategoryByIdHandler
        : IRequestHandler<GetCategoryByIdQuery, Entities.Category?>
    {
        private readonly Repositories.ICategoryRepository _categoryRepo;

        public GetCategoryByIdHandler(Repositories.ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<Entities.Category?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            return await _categoryRepo.Query()
                .Include(c => c.Products)
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.IsActive, cancellationToken);
        }
    }
}
