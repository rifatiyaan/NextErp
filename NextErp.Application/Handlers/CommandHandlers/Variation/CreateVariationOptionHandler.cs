using MediatR;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Variation
{
    public class CreateVariationOptionHandler(IApplicationDbContext dbContext)
        : IRequestHandler<CreateVariationOptionCommand, int>
    {
        public async Task<int> Handle(CreateVariationOptionCommand request, CancellationToken cancellationToken)
        {
            var product = await dbContext.Products.FindAsync([request.ProductId], cancellationToken);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {request.ProductId} not found.");

            var option = new Entities.VariationOption
            {
                Title = request.Name,
                Name = request.Name,
                ProductId = request.ProductId,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                TenantId = product.TenantId,
                BranchId = product.BranchId
            };

            await dbContext.VariationOptions.AddAsync(option, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return option.Id;
        }
    }
}

