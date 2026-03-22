using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class UpdateProductHandler(
        IApplicationUnitOfWork unitOfWork,
        IApplicationDbContext dbContext,
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

            if (!existing.HasVariations)
                await SyncDefaultVariantWithSimpleProductAsync(existing.Id, existing.Price, existing.Stock, cancellationToken);

            await unitOfWork.SaveAsync();

            return Unit.Value;
        }

        private async Task SyncDefaultVariantWithSimpleProductAsync(
            int productId,
            decimal price,
            int stock,
            CancellationToken cancellationToken)
        {
            var def = await dbContext.ProductVariants
                .Where(pv => pv.ProductId == productId)
                .OrderBy(pv => pv.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (def == null)
                return;

            def.Price = price;
            def.Stock = stock;
            def.UpdatedAt = DateTime.UtcNow;

            var stockRow = await dbContext.Stocks.FirstOrDefaultAsync(s => s.Id == def.Id, cancellationToken);
            if (stockRow == null)
                return;

            stockRow.AvailableQuantity = stock;
            stockRow.UpdatedAt = DateTime.UtcNow;
        }
    }
}
