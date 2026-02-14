using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class UpdateProductHandler(
        IApplicationUnitOfWork unitOfWork,
        IMapper mapper)
        : IRequestHandler<UpdateProductCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var existing = await unitOfWork.ProductRepository.Query()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            
            if (existing == null)
                throw new KeyNotFoundException($"Product with ID {request.Id} not found.");

            mapper.Map(request, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.ProductRepository.EditAsync(existing);
            await unitOfWork.SaveAsync();

            return Unit.Value;
        }
    }
}
