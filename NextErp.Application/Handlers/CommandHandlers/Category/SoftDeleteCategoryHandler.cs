using NextErp.Application.Commands;
using MediatR;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.CommandHandlers.Category
{
    public class SoftDeleteCategoryHandler(Repositories.ICategoryRepository categoryRepo)
        : IRequestHandler<SoftDeleteCategoryCommand, Unit>
    {
        public async Task<Unit> Handle(SoftDeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await categoryRepo.GetByIdAsync(request.Id);
            if (category != null && category.IsActive)
            {
                category.IsActive = false;
                category.UpdatedAt = DateTime.UtcNow;
                await categoryRepo.EditAsync(category);
            }
            return Unit.Value;
        }
    }
}
