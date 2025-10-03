using MediatR;
using Commands = EcommerceApplicationWeb.Application.Features.Categories.Commands;
using Entities = EcommerceApplicationWeb.Domain.Entities;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Features.Handlers.Categories
{
    public class CreateCategoryHandler : IRequestHandler<Commands.CreateCategoryCommand, int>
    {
        private readonly Repositories.ICategoryRepository _categoryRepo;

        public CreateCategoryHandler(Repositories.ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<int> Handle(Commands.CreateCategoryCommand request, CancellationToken cancellationToken)
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
