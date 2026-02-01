using MediatR;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Variation
{
    public class UpdateVariationValueHandler(IApplicationDbContext dbContext)
        : IRequestHandler<UpdateVariationValueCommand>
    {
        public async Task Handle(UpdateVariationValueCommand request, CancellationToken cancellationToken)
        {
            var value = await dbContext.VariationValues.FindAsync([request.Id], cancellationToken);
            if (value == null)
                throw new InvalidOperationException($"Variation value with ID {request.Id} not found.");

            value.Value = request.Value;
            value.Name = request.Value;
            value.Title = request.Value;
            value.DisplayOrder = request.DisplayOrder;
            value.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

