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
            var query = productRepo.Query();

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                switch (request.Status.ToLower())
                {
                    case "active":
                        query = query.Where(p => p.IsActive);
                        break;
                    case "out of stock":
                        query = query.Where(p => p.IsActive && p.Stock == 0);
                        break;
                    case "closed":
                        query = query.Where(p => !p.IsActive);
                        break;
                    default:
                        // "all" or unknown - show all active products by default
                        query = query.Where(p => p.IsActive);
                        break;
                }
            }
            else
            {
                // Default: show only active products
                query = query.Where(p => p.IsActive);
            }

            // Apply search text filter (search in title and code)
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var searchText = request.SearchText.Trim();
                query = query.Where(p => 
                    p.Title.Contains(searchText) || 
                    p.Code.Contains(searchText));
            }

            // Apply category filter
            if (request.CategoryId.HasValue && request.CategoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);
            }

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
