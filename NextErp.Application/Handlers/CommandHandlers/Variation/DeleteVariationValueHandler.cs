using MediatR;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Variation
{
    public class DeleteVariationValueHandler(IApplicationDbContext dbContext)
        : IRequestHandler<DeleteVariationValueCommand>
    {
        public async Task Handle(DeleteVariationValueCommand request, CancellationToken cancellationToken)
        {
            var value = await dbContext.VariationValues.FindAsync([request.Id], cancellationToken);
            if (value == null)
                throw new InvalidOperationException($"Variation value with ID {request.Id} not found.");

            dbContext.VariationValues.Remove(value);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

