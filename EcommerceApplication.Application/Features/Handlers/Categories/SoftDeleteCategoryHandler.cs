using MediatR;
using Commands = EcommerceApplicationWeb.Application.Features.Categories.Commands;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Features.Handlers.Category
{
    public class SoftDeleteCategoryHandler
        : IRequestHandler<Commands.SoftDeleteCategoryCommand, Unit>
    {
        private readonly Repositories.ICategoryRepository _categoryRepo;

        public SoftDeleteCategoryHandler(Repositories.ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<Unit> Handle(Commands.SoftDeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepo.GetByIdAsync(request.Id);
            if (category != null && category.IsActive)
            {
                category.IsActive = false;
                category.UpdatedAt = DateTime.UtcNow;
                await _categoryRepo.EditAsync(category);
            }
            return Unit.Value;
        }
    }
}
