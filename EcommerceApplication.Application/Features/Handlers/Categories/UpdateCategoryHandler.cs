using MediatR;
using Commands = EcommerceApplicationWeb.Application.Features.Categories.Commands;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Features.Handlers.Category
{
    public class UpdateCategoryHandler
        : IRequestHandler<Commands.UpdateCategoryCommand, Unit>
    {
        private readonly Repositories.ICategoryRepository _categoryRepo;

        public UpdateCategoryHandler(Repositories.ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<Unit> Handle(Commands.UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var existing = await _categoryRepo.GetByIdAsync(request.Id);
            if (existing != null && existing.IsActive)
            {
                existing.Title = request.Title;
                existing.Description = request.Description;
                existing.ParentId = request.ParentId;
                existing.UpdatedAt = DateTime.UtcNow;

                await _categoryRepo.EditAsync(existing);
            }

            return Unit.Value;
        }
    }
}
