using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.CommandHandlers.Category
{
    public class UpdateCategoryHandler(
        IApplicationUnitOfWork unitOfWork,
        IMapper mapper)
        : IRequestHandler<UpdateCategoryCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var existing = await unitOfWork.CategoryRepository.GetByIdAsync(request.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Category with ID {request.Id} not found.");

            mapper.Map(request, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.CategoryRepository.EditAsync(existing);
            await unitOfWork.SaveAsync();

            return Unit.Value;
        }
    }
}
