using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Common.Settings;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Application.Settings;

namespace NextErp.Application.Handlers.QueryHandlers.Ecommerce;

// Recent approved reviews across all storefront-visible products — powers the
// homepage testimonial wall. Scoped exactly like the other store reads: only
// reviews on published, active products in the selling branch (with a published
// category). IgnoreQueryFilters is required because anonymous store requests
// carry no branch claim, so the [BranchScoped] filter on Product would otherwise
// zero the join and drop every row.
public class GetRecentReviewsHandler(IApplicationDbContext dbContext, ISettingsProvider settings)
    : IRequestHandler<GetRecentReviewsQuery, List<StoreRecentReviewRow>>
{
    public async Task<List<StoreRecentReviewRow>> Handle(GetRecentReviewsQuery request, CancellationToken cancellationToken = default)
    {
        var branchId = await StoreQueryShared.SellingBranchAsync(settings, dbContext, cancellationToken);
        var visibleProductIds = StoreQueryShared.PublishedProducts(dbContext, branchId, null).Select(p => p.Id);

        return await dbContext.Reviews
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.IsApproved
                        && r.Text != null && r.Text != ""
                        && visibleProductIds.Contains(r.ProductId))
            .OrderByDescending(r => r.CreatedAt)
            .Take(request.Take)
            .Select(r => new StoreRecentReviewRow(
                r.Id, r.AuthorName, r.Rating, r.Text, r.CreatedAt, r.ProductId, r.Product.Title))
            .ToListAsync(cancellationToken);
    }
}
