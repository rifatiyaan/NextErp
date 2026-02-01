using NextErp.Application.Common;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.QueryHandlers.Product
{
    public class GetPagedProductsHandler(Repositories.IProductRepository productRepo) 
        : IRequestHandler<GetPagedProductsQuery, PagedResult<Entities.Product>>
    {
        public async Task<PagedResult<Entities.Product>> Handle(
            GetPagedProductsQuery request,
            CancellationToken cancellationToken)
        {
            var query = productRepo.Query().Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                query = query.Where(p => p.Title.Contains(request.SearchText));

            var total = await query.CountAsync(cancellationToken);

            query = request.SortBy?.ToLower() switch
            {
                "title" => query.OrderBy(p => p.Title),
                "price" => query.OrderBy(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var records = await query
                .Include(p => p.Category)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Entities.Product>(records, total, total);
        }
    }
}
