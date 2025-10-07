using EcommerceApplicationWeb.Application.Commands;
using MediatR;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.CommandHandlers.Category
{
    public class UpdateCategoryHandler
        : IRequestHandler<UpdateCategoryCommand, Unit>
    {
        private readonly Repositories.ICategoryRepository _categoryRepo;

        public UpdateCategoryHandler(Repositories.ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
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
