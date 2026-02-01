using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Variation
{
    public class CreateVariationValueHandler(IApplicationDbContext dbContext)
        : IRequestHandler<CreateVariationValueCommand, int>
    {
        public async Task<int> Handle(CreateVariationValueCommand request, CancellationToken cancellationToken)
        {
            var option = await dbContext.VariationOptions
                .Include(vo => vo.Product)
                .FirstOrDefaultAsync(vo => vo.Id == request.VariationOptionId, cancellationToken);
            
            if (option == null)
                throw new InvalidOperationException($"Variation option with ID {request.VariationOptionId} not found.");

            var value = new Entities.VariationValue
            {
                Title = request.Value,
                Name = request.Value,
                Value = request.Value,
                VariationOptionId = request.VariationOptionId,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                TenantId = option.TenantId,
                BranchId = option.BranchId
            };

            await dbContext.VariationValues.AddAsync(value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return value.Id;
        }
    }
}

