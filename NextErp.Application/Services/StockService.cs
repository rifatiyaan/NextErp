using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;

namespace NextErp.Application.Services
{
    public class StockService(
        IStockRepository stockRepository,
        IApplicationDbContext dbContext,
        IBranchProvider branchProvider)
        : IStockService
    {
        public Task<bool> CheckStockAvailabilityAsync(int productVariantId, decimal requiredQuantity, CancellationToken cancellationToken = default)
        {
            if (requiredQuantity <= 0)
                return Task.FromResult(true);

            return CheckAvailabilityInternalAsync(productVariantId, requiredQuantity, cancellationToken);
        }

        public Task<decimal> GetAvailableStockAsync(int productVariantId, CancellationToken cancellationToken = default)
        {
            return ResolveAvailableQuantityAsync(productVariantId, cancellationToken);
        }

        public async Task ReduceStockAsync(int productVariantId, decimal quantity, CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                return;

            var variant = await RequireVariantAsync(productVariantId, cancellationToken);
            var branchId = branchProvider.GetRequiredBranchId();
            await RecordMovementAsync(
                productVariantId,
                variant.TenantId,
                branchId,
                -quantity,
                StockMovementType.Adjustment,
                Guid.Empty,
                cancellationToken);
        }

        public async Task IncreaseStockAsync(int productVariantId, decimal quantity, CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                return;

            var variant = await RequireVariantAsync(productVariantId, cancellationToken);
            var branchId = branchProvider.GetRequiredBranchId();
            await RecordMovementAsync(
                productVariantId,
                variant.TenantId,
                branchId,
                quantity,
                StockMovementType.Adjustment,
                Guid.Empty,
                cancellationToken);
        }

        public async Task EnsureStockRecordExistsAsync(int productVariantId, CancellationToken cancellationToken = default)
        {
            var variant = await RequireVariantAsync(productVariantId, cancellationToken);
            var branchId = branchProvider.GetRequiredBranchId();
            _ = await UpsertStockRowAsync(productVariantId, variant.TenantId, branchId, cancellationToken);
        }

        public async Task RecordMovementAsync(
            int productVariantId,
            Guid tenantId,
            Guid branchId,
            decimal quantityDelta,
            StockMovementType type,
            Guid referenceId,
            CancellationToken cancellationToken = default)
        {
            if (quantityDelta == 0)
                return;

            var variant = await RequireVariantAsync(productVariantId, cancellationToken);

            var stock = await stockRepository.GetByProductVariantIdAndBranchIdAsync(
                productVariantId,
                branchId,
                cancellationToken);

            if (stock == null)
            {
                if (quantityDelta < 0)
                    throw InsufficientStock(variant, 0, -quantityDelta);

                stock = CreateStockRow(variant, tenantId, branchId, initialAvailable: 0);
                await stockRepository.AddAsync(stock);
            }

            var projected = stock.AvailableQuantity + quantityDelta;
            if (projected < 0)
                throw InsufficientStock(variant, stock.AvailableQuantity, -quantityDelta);

            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductVariantId = productVariantId,
                BranchId = branchId,
                Quantity = quantityDelta,
                Type = type,
                ReferenceId = referenceId,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.StockMovements.AddAsync(movement, cancellationToken);

            ApplyQuantityChange(stock, variant, quantityDelta);
            await stockRepository.EditAsync(stock);
            await SyncProductAggregateStockAsync(variant.ProductId, cancellationToken);
        }

        private async Task<bool> CheckAvailabilityInternalAsync(int productVariantId, decimal requiredQuantity, CancellationToken cancellationToken)
        {
            var available = await ResolveAvailableQuantityAsync(productVariantId, cancellationToken);
            return available >= requiredQuantity;
        }

        private async Task<decimal> ResolveAvailableQuantityAsync(int productVariantId, CancellationToken cancellationToken)
        {
            var variant = await RequireVariantAsync(productVariantId, cancellationToken);
            var branchId = branchProvider.GetRequiredBranchId();
            var stock = await UpsertStockRowAsync(productVariantId, variant.TenantId, branchId, cancellationToken);
            return stock.AvailableQuantity;
        }

        private async Task<ProductVariant> RequireVariantAsync(int productVariantId, CancellationToken cancellationToken)
        {
            var variant = await dbContext.ProductVariants
                .FirstOrDefaultAsync(pv => pv.Id == productVariantId, cancellationToken);

            if (variant == null)
                throw new InvalidOperationException($"Product variant {productVariantId} was not found.");

            return variant;
        }

        private async Task<Stock> UpsertStockRowAsync(
            int productVariantId,
            Guid tenantId,
            Guid branchId,
            CancellationToken cancellationToken)
        {
            var stock = await stockRepository.GetByProductVariantIdAndBranchIdAsync(
                productVariantId,
                branchId,
                cancellationToken);
            if (stock != null)
                return stock;

            var variant = await RequireVariantAsync(productVariantId, cancellationToken);
            var row = CreateStockRow(variant, tenantId, branchId, initialAvailable: 0);
            await stockRepository.AddAsync(row);
            return row;
        }

        private static Stock CreateStockRow(ProductVariant variant, Guid tenantId, Guid branchId, decimal initialAvailable)
        {
            return new Stock
            {
                Id = Guid.NewGuid(),
                Title = $"Stock — {variant.Sku}",
                ProductVariantId = variant.Id,
                AvailableQuantity = initialAvailable,
                TenantId = tenantId,
                BranchId = branchId,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static void ApplyQuantityChange(Stock stock, ProductVariant variant, decimal delta)
        {
            stock.AvailableQuantity += delta;
            if (stock.AvailableQuantity < 0)
                stock.AvailableQuantity = 0;

            stock.UpdatedAt = DateTime.UtcNow;
            variant.Stock = (int)Math.Floor(stock.AvailableQuantity);
            variant.UpdatedAt = DateTime.UtcNow;
        }

        private async Task SyncProductAggregateStockAsync(int productId, CancellationToken cancellationToken)
        {
            var product = await dbContext.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

            if (product == null)
                return;

            product.Stock = product.ProductVariants.Sum(pv => pv.Stock);
            product.UpdatedAt = DateTime.UtcNow;
        }

        private static InvalidOperationException InsufficientStock(ProductVariant variant, decimal available, decimal required)
        {
            return new InvalidOperationException(
                $"Insufficient stock for SKU '{variant.Sku}'. Available: {available}, Required: {required}.");
        }
    }
}
