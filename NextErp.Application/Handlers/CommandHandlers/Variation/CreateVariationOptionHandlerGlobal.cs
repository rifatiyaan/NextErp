using MediatR;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Variation
{
    public class CreateVariationOptionHandlerGlobal(IApplicationDbContext dbContext)
        : IRequestHandler<CreateVariationOptionCommandGlobal, int>
    {
        public async Task<int> Handle(CreateVariationOptionCommandGlobal request, CancellationToken cancellationToken)
        {
            var option = new Entities.VariationOption
            {
                Name = request.Name,
                Title = request.Name,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                TenantId = request.TenantId,
                BranchId = request.BranchId
            };

            await dbContext.VariationOptions.AddAsync(option, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return option.Id;
        }
    }
}
