using NextErp.Application.Common;
using NextErp.Application.Common.Extensions;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Category
{
    public class GetPagedCategoriesHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetPagedCategoriesQuery, PagedResult<Entities.Category>>
    {
        public async Task<PagedResult<Entities.Category>> Handle(
            GetPagedCategoriesQuery request,
            CancellationToken cancellationToken = default)
        {
            var query = dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .WhereIfNotEmpty(request.SearchText, c => c.Title.Contains(request.SearchText!));

            var total = await query.CountAsync(cancellationToken);

            query = request.SortBy?.ToLower() switch
            {
                "title" => query.OrderBy(c => c.Title),
                "createdat" => query.OrderBy(c => c.CreatedAt),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            var records = await query
                .Include(c => c.Parent)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Entities.Category>(records, total, total);
        }
    }
}
