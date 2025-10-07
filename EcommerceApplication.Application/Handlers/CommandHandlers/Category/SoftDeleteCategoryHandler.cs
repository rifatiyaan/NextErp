using EcommerceApplicationWeb.Application.Commands;
using MediatR;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.CommandHandlers.Category
{
    public class SoftDeleteCategoryHandler
        : IRequestHandler<SoftDeleteCategoryCommand, Unit>
    {
        private readonly Repositories.ICategoryRepository _categoryRepo;

        public SoftDeleteCategoryHandler(Repositories.ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<Unit> Handle(SoftDeleteCategoryCommand request, CancellationToken cancellationToken)
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
