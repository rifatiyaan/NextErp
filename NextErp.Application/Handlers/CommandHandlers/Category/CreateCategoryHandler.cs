using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Category
{
    public class CreateCategoryHandler(
        IApplicationDbContext dbContext,
        IMapper mapper)
        : IRequestHandler<CreateCategoryCommand, int>
    {
        public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken = default)
        {
            var category = mapper.Map<Entities.Category>(request);
            category.IsActive = true;
            category.CreatedAt = DateTime.UtcNow;

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync(cancellationToken);
            return category.Id;
        }
    }
}
