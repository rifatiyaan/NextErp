using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Ecommerce;

namespace NextErp.Application.Handlers.QueryHandlers.Ecommerce;

public class GetEcommercePublicationHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetEcommercePublicationQuery, List<PublicationCategoryResponse>>
{
    public async Task<List<PublicationCategoryResponse>> Handle(
        GetEcommercePublicationQuery request, CancellationToken cancellationToken = default)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Title)
            .Select(c => new { c.Id, c.Title, c.ParentId, c.IsPublishedOnline })
            .ToListAsync(cancellationToken);

        // Admin curation view intentionally spans branches: one batched product
        // query, grouped in memory (no N+1).
        var products = await dbContext.Products
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new { p.Id, p.Title, p.Code, p.Price, p.IsPublishedOnline, p.CategoryId })
            .ToListAsync(cancellationToken);
        var byCategory = products.GroupBy(p => p.CategoryId).ToDictionary(g => g.Key, g => g.ToList());

        return categories.Select(c => new PublicationCategoryResponse(
            c.Id, c.Title, c.ParentId, c.IsPublishedOnline,
            (byCategory.GetValueOrDefault(c.Id) ?? new())
                .Select(p => new PublicationProductRow(p.Id, p.Title, p.Code, p.Price, p.IsPublishedOnline))
                .ToList()))
            .ToList();
    }
}
