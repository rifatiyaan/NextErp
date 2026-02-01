using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Variation
{
    public class DeleteVariationOptionHandler(IApplicationDbContext dbContext)
        : IRequestHandler<DeleteVariationOptionCommand>
    {
        public async Task Handle(DeleteVariationOptionCommand request, CancellationToken cancellationToken)
        {
            var option = await dbContext.VariationOptions
                .Include(vo => vo.Values)
                .FirstOrDefaultAsync(vo => vo.Id == request.Id, cancellationToken);
            
            if (option == null)
                throw new InvalidOperationException($"Variation option with ID {request.Id} not found.");

            dbContext.VariationOptions.Remove(option);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

