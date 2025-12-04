using AutoMapper;
using EcommerceApplicationWeb.Application.Commands;
using MediatR;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.CommandHandlers.Category
{
    public class UpdateCategoryHandler(
        Repositories.ICategoryRepository categoryRepo,
        IMapper mapper)
        : IRequestHandler<UpdateCategoryCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var existing = await categoryRepo.GetByIdAsync(request.Id);
            if (existing != null && existing.IsActive)
            {
                mapper.Map(request, existing);
                existing.UpdatedAt = DateTime.UtcNow;

                await categoryRepo.EditAsync(existing);
            }

            return Unit.Value;
        }
    }
}
