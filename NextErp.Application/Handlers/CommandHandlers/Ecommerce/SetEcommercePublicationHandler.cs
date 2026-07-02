using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.Ecommerce;

public class SetEcommercePublicationHandler(IApplicationDbContext dbContext)
    : IRequestHandler<SetEcommercePublicationCommand, Unit>
{
    public async Task<Unit> Handle(SetEcommercePublicationCommand request, CancellationToken cancellationToken = default)
    {
        var categoryIds = request.PublishCategoryIds.Concat(request.UnpublishCategoryIds).ToList();
        var categories = await dbContext.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);
        foreach (var category in categories)
            category.IsPublishedOnline = request.PublishCategoryIds.Contains(category.Id);

        var productIds = request.PublishProductIds.Concat(request.UnpublishProductIds).ToList();
        var products = await dbContext.Products
            .IgnoreQueryFilters()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
        foreach (var product in products)
            product.IsPublishedOnline = request.PublishProductIds.Contains(product.Id);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
