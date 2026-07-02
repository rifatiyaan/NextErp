using NextErp.Application.Commands;
using MediatR;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;

namespace NextErp.Application.Handlers.CommandHandlers.Category
{
    public class CreateCategoryHandler(
        IApplicationDbContext dbContext)
        : IRequestHandler<CreateCategoryCommand, int>
    {
        public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken = default)
        {
            var category = request.ToEntity();
            category.IsActive = true;
            category.CreatedAt = DateTime.UtcNow;

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync(cancellationToken);
            return category.Id;
        }
    }
}
