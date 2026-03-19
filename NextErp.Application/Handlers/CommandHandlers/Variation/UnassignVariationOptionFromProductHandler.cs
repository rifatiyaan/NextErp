using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.Variation
{
    public class UnassignVariationOptionFromProductHandler(IApplicationDbContext dbContext)
        : IRequestHandler<UnassignVariationOptionFromProductCommand>
    {
        public async Task Handle(UnassignVariationOptionFromProductCommand request, CancellationToken cancellationToken)
        {
            var pvo = await dbContext.ProductVariationOptions
                .FirstOrDefaultAsync(pvo => pvo.ProductId == request.ProductId && pvo.VariationOptionId == request.VariationOptionId, cancellationToken);

            if (pvo == null)
                throw new InvalidOperationException("This variation option is not assigned to the product.");

            dbContext.ProductVariationOptions.Remove(pvo);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
