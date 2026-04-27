using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.Category
{
    public class UpdateCategoryHandler(
        IApplicationDbContext dbContext,
        IMapper mapper)
        : IRequestHandler<UpdateCategoryCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (existing == null)
                throw new KeyNotFoundException($"Category with ID {request.Id} not found.");

            mapper.Map(request, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
