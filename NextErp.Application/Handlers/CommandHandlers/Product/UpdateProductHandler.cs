using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Products;
using Repositories = NextErp.Domain.Repositories;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class UpdateProductHandler(
        IApplicationUnitOfWork unitOfWork,
        IApplicationDbContext dbContext,
        IStockService stockService,
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

            if (request.ImageGallery != null)
            {
                await ProductGallerySync.ApplyFullGalleryAsync(
                    existing,
                    request.ImageGallery,
                    dbContext,
                    cancellationToken);
            }
            else if (request.ImageThumbnailUpdates is { Count: > 0 })
            {
                await ProductGallerySync.ApplyThumbnailUpdatesAsync(
                    request.Id,
                    request.ImageThumbnailUpdates,
                    existing,
                    dbContext,
                    cancellationToken);
            }

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
            def.UpdatedAt = DateTime.UtcNow;

            await stockService.EnsureStockRecordExistsAsync(def.Id, cancellationToken);
            await stockService.SetAvailableQuantityAsync(def.Id, stock, cancellationToken);
        }
    }
}
