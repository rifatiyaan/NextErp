using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class UpdateProductHandler(
        Repositories.IProductRepository productRepo,
        IMapper mapper)
        : IRequestHandler<UpdateProductCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var existing = await productRepo.GetByIdAsync(request.Id);
            if (existing != null && existing.IsActive)
            {
                mapper.Map(request, existing);
                existing.UpdatedAt = DateTime.UtcNow;

                await productRepo.EditAsync(existing);
            }

            return Unit.Value;
        }
    }
}
