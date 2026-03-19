using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Variation
{
    public class AssignVariationOptionToProductHandler(IApplicationDbContext dbContext)
        : IRequestHandler<AssignVariationOptionToProductCommand, int>
    {
        public async Task<int> Handle(AssignVariationOptionToProductCommand request, CancellationToken cancellationToken)
        {
            var productExists = await dbContext.Products.AnyAsync(p => p.Id == request.ProductId, cancellationToken);
            if (!productExists)
                throw new InvalidOperationException($"Product with ID {request.ProductId} not found.");

            var option = await dbContext.VariationOptions.FindAsync([request.VariationOptionId], cancellationToken);
            if (option == null)
                throw new InvalidOperationException($"Variation option with ID {request.VariationOptionId} not found.");

            var alreadyAssigned = await dbContext.ProductVariationOptions
                .AnyAsync(pvo => pvo.ProductId == request.ProductId && pvo.VariationOptionId == request.VariationOptionId, cancellationToken);
            if (alreadyAssigned)
                throw new InvalidOperationException("This variation option is already assigned to the product.");

            var pvo = new Entities.ProductVariationOption
            {
                Title = option.Name,
                ProductId = request.ProductId,
                VariationOptionId = request.VariationOptionId,
                DisplayOrder = request.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.ProductVariationOptions.AddAsync(pvo, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return pvo.Id;
        }
    }
}
