using MediatR;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Variation
{
    public class UpdateVariationOptionHandler(IApplicationDbContext dbContext)
        : IRequestHandler<UpdateVariationOptionCommand>
    {
        public async Task Handle(UpdateVariationOptionCommand request, CancellationToken cancellationToken)
        {
            var option = await dbContext.VariationOptions.FindAsync([request.Id], cancellationToken);
            if (option == null)
                throw new InvalidOperationException($"Variation option with ID {request.Id} not found.");

            option.Name = request.Name;
            option.Title = request.Name;
            option.DisplayOrder = request.DisplayOrder;
            option.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

