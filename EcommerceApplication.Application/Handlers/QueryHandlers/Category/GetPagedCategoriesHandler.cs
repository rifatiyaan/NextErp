using EcommerceApplicationWeb.Application.Common;
using EcommerceApplicationWeb.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.QueryHandlers.Category
{
    public class GetPagedCategoriesHandler(Repositories.ICategoryRepository categoryRepo)
        : IRequestHandler<GetPagedCategoriesQuery, PagedResult<Entities.Category>>
    {
        public async Task<PagedResult<Entities.Category>> Handle(
            GetPagedCategoriesQuery request,
            CancellationToken cancellationToken)
        {
            var query = categoryRepo.Query().Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(c => c.Title.Contains(request.SearchText));

            var total = await query.CountAsync(cancellationToken);

            query = request.SortBy?.ToLower() switch
            {
                "title" => query.OrderBy(c => c.Title),
                "createdat" => query.OrderBy(c => c.CreatedAt),
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
