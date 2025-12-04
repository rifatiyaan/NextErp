using AutoMapper;
using EcommerceApplicationWeb.Application.Commands;
using MediatR;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.CommandHandlers.Category
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
