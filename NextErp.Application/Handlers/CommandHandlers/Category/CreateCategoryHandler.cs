using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Entities = NextErp.Domain.Entities;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.CommandHandlers.Category
{
    public class CreateCategoryHandler(
        Repositories.ICategoryRepository categoryRepo,
        IMapper mapper) 
        : IRequestHandler<CreateCategoryCommand, int>
    {
        public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = mapper.Map<Entities.Category>(request);
            category.CreatedAt = DateTime.UtcNow;

            await categoryRepo.AddAsync(category);
            return category.Id;
        }
    }
}
