using NextErp.Application.Commands;
using MediatR;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.CommandHandlers.Category
{
    public class SoftDeleteCategoryHandler(IApplicationUnitOfWork unitOfWork)
        : IRequestHandler<SoftDeleteCategoryCommand, Unit>
    {
        public async Task<Unit> Handle(SoftDeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await unitOfWork.CategoryRepository.GetByIdAsync(request.Id);
            if (category != null && category.IsActive)
            {
                category.IsActive = false;
                category.UpdatedAt = DateTime.UtcNow;
                await unitOfWork.CategoryRepository.EditAsync(category);
                await unitOfWork.SaveAsync();
            }
            return Unit.Value;
        }
    }
}
