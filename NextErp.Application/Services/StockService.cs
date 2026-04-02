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
            var stock = await UpsertStockRowAsync(productVariantId, variant.TenantId, cancellationToken);
            var available = stock.AvailableQuantity;

            if (available < quantity)
                throw InsufficientStock(variant, available, quantity);

            ApplyQuantityChange(stock, variant, -quantity);
            await stockRepository.EditAsync(stock);
            await SyncProductAggregateStockAsync(variant.ProductId, cancellationToken);
        }

        public async Task IncreaseStockAsync(int productVariantId, decimal quantity, CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                return;

            var variant = await RequireVariantAsync(productVariantId, cancellationToken);
            var stock = await UpsertStockRowAsync(productVariantId, variant.TenantId, cancellationToken);

            ApplyQuantityChange(stock, variant, quantity);
            await stockRepository.EditAsync(stock);
            await SyncProductAggregateStockAsync(variant.ProductId, cancellationToken);
        }

        public async Task EnsureStockRecordExistsAsync(int productVariantId, CancellationToken cancellationToken = default)
        {
            var variant = await RequireVariantAsync(productVariantId, cancellationToken);
            _ = await UpsertStockRowAsync(productVariantId, variant.TenantId, cancellationToken);
        }

        private async Task<bool> CheckAvailabilityInternalAsync(int productVariantId, decimal requiredQuantity, CancellationToken cancellationToken)
        {
            var available = await ResolveAvailableQuantityAsync(productVariantId, cancellationToken);
            return available >= requiredQuantity;
        }

        private async Task<decimal> ResolveAvailableQuantityAsync(int productVariantId, CancellationToken cancellationToken)
        {
            var variant = await RequireVariantAsync(productVariantId, cancellationToken);
            var stock = await UpsertStockRowAsync(productVariantId, variant.TenantId, cancellationToken);
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

        private async Task<Stock> UpsertStockRowAsync(int productVariantId, Guid tenantId, CancellationToken cancellationToken)
        {
            var stock = await stockRepository.GetByProductVariantIdAsync(productVariantId, cancellationToken);
            if (stock != null)
                return stock;

            var variant = await RequireVariantAsync(productVariantId, cancellationToken);
            var row = CreateStockRow(variant, tenantId, branchProvider.GetRequiredBranchId());
            await stockRepository.AddAsync(row);
            return row;
        }

        private static Stock CreateStockRow(ProductVariant variant, Guid tenantId, Guid branchId)
        {
            return new Stock
            {
                Id = Guid.NewGuid(),
                Title = $"Stock — {variant.Sku}",
                ProductVariantId = variant.Id,
                AvailableQuantity = variant.Stock > 0 ? variant.Stock : 0,
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
