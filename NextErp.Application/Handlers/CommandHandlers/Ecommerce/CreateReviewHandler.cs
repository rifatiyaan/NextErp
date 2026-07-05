using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Ecommerce;

public class CreateReviewHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateReviewCommand, int>
{
    public async Task<int> Handle(CreateReviewCommand request, CancellationToken cancellationToken = default)
    {
        // Only publicly-visible products (active + published) can be reviewed.
        // Anonymous request carries no branch claim, so bypass the scope filter.
        var product = await dbContext.Products
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.Id == request.ProductId && p.IsActive && p.IsPublishedOnline)
            .Select(p => new { p.Id, p.TenantId })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            throw new ValidationException(new[]
            {
                new ValidationFailure("ProductId", "This product is not available for review."),
            });

        var review = new Entities.Review
        {
            ProductId = product.Id,
            AuthorName = request.AuthorName.Trim(),
            Rating = request.Rating,
            Text = request.Text.Trim(),
            IsApproved = true,
            TenantId = product.TenantId,
            CreatedAt = DateTime.UtcNow,
        };

        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync(cancellationToken);
        return review.Id;
    }
}
