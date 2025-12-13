using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Entities = NextErp.Domain.Entities;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class CreateProductHandler(
        IApplicationUnitOfWork unitOfWork,
        IMapper mapper) 
        : IRequestHandler<CreateProductCommand, int>
    {
        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = mapper.Map<Entities.Product>(request);
            product.IsActive = true;
            product.CreatedAt = DateTime.UtcNow;

            await unitOfWork.ProductRepository.AddAsync(product);
            await unitOfWork.SaveAsync();
            return product.Id;
        }
    }
}
