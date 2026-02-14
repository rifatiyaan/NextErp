using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Entities = NextErp.Domain.Entities;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.CommandHandlers.Category
{
    public class CreateCategoryHandler(
        IApplicationUnitOfWork unitOfWork,
        IMapper mapper) 
        : IRequestHandler<CreateCategoryCommand, int>
    {
        public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = mapper.Map<Entities.Category>(request);
            category.IsActive = true;
            category.CreatedAt = DateTime.UtcNow;

            await unitOfWork.CategoryRepository.AddAsync(category);
            await unitOfWork.SaveAsync();
            return category.Id;
        }
    }
}
