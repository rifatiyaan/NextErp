using EcommerceApplicationWeb.Application.Commands;
using MediatR;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.CommandHandlers.Category
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, int>
    {
        private readonly Repositories.ICategoryRepository _categoryRepo;

        public CreateCategoryHandler(Repositories.ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = new Entities.Category
            {
                Title = request.Title,
                Description = request.Description,
                ParentId = request.ParentId,
                CreatedAt = DateTime.UtcNow
            };

            await _categoryRepo.AddAsync(category);
            return category.Id;
        }
    }
}
