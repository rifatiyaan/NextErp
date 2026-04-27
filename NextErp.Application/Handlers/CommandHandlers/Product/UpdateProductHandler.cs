using AutoMapper;
using NextErp.Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Products;

namespace NextErp.Application.Handlers.CommandHandlers.Product
{
    public class UpdateProductHandler(
        IApplicationDbContext dbContext,
        IStockService stockService,
        IMapper mapper)
        : IRequestHandler<UpdateProductCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.Products
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

            // Tracked entity — change tracker will pick up modifications without an explicit Update call.

            if (!existing.HasVariations)
                await SyncDefaultVariantPriceAsync(existing.Id, existing.Price, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        // Stock changes flow only through StockMovement; product update keeps Stock rows untouched.
        private async Task SyncDefaultVariantPriceAsync(
            int productId,
            decimal price,
            CancellationToken cancellationToken = default)
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
        }
    }
}
