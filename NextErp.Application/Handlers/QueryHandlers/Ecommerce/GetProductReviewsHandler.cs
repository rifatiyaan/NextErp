using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Ecommerce;

namespace NextErp.Application.Handlers.QueryHandlers.Ecommerce;

public class GetProductReviewsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetProductReviewsQuery, StoreReviewsResponse>
{
    public async Task<StoreReviewsResponse> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken = default)
    {
        var reviews = await dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == request.ProductId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new StoreReviewRow(r.Id, r.AuthorName, r.Rating, r.Text, r.CreatedAt))
            .ToListAsync(cancellationToken);

        var count = reviews.Count;
        var average = count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 1);
        return new StoreReviewsResponse(average, count, reviews);
    }
}
