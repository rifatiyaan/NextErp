using EcommerceApplicationWeb.Application.Commands;
using MediatR;
using Repositories = EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application.Handlers.CommandHandlers.Product
{
    public class UpdateProductHandler(Repositories.IProductRepository productRepo)
        : IRequestHandler<UpdateProductCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var existing = await productRepo.GetByIdAsync(request.Id);
            if (existing != null && existing.IsActive)
            {
                existing.Title = request.Title;
                existing.Code = request.Code;
                existing.ParentId = request.ParentId;
                existing.CategoryId = request.CategoryId;
                existing.Price = request.Price;
                existing.Stock = request.Stock;
                existing.ImageUrl = request.ImageUrl;
                existing.Metadata.Description = request.Description;
                existing.Metadata.Color = request.Color;
                existing.Metadata.Warranty = request.Warranty;

                existing.UpdatedAt = DateTime.UtcNow;

                await productRepo.EditAsync(existing);
            }

            return Unit.Value;
        }
    }
}
