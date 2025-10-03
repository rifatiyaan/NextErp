using EcommerceApplicationWeb.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Queries = EcommerceApplicationWeb.Application.Features.Categories.Queries;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Features.Handlers.Category
{
    public class GetPagedCategoriesHandler
        : IRequestHandler<Queries.GetPagedCategoriesQuery, PagedResult<Entities.Category>>
    {
        private readonly Repositories.ICategoryRepository _categoryRepo;

        public GetPagedCategoriesHandler(Repositories.ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<PagedResult<Entities.Category>> Handle(
            Queries.GetPagedCategoriesQuery request,
            CancellationToken cancellationToken)
        {
            var query = _categoryRepo.Query().Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(c => c.Title.Contains(request.SearchText));

            var total = await query.CountAsync(cancellationToken);

            query = request.SortBy?.ToLower() switch
            {
                "title" => query.OrderBy(c => c.Title),
                _ => query.OrderBy(c => c.Id)
            };

            var records = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Entities.Category>(records, total, total);
        }
    }
}
