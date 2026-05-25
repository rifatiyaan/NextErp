using NextErp.Application.Services;
using NextErp.Application.Settings;
using NextErp.Domain.Entities;

namespace NextErp.Application.Interfaces
{
    public interface IStockService
    {
        // ---- Single-variant async API (kept for single-item handlers) ----

        Task<bool> CheckStockAvailabilityAsync(int productVariantId, decimal requiredQuantity, CancellationToken cancellationToken = default);

        Task<decimal> GetAvailableStockAsync(int productVariantId, CancellationToken cancellationToken = default);

        Task SetAvailableQuantityAsync(int productVariantId, decimal targetQuantity, CancellationToken cancellationToken = default);

        Task ReduceStockAsync(int productVariantId, decimal quantity, CancellationToken cancellationToken = default);

        Task IncreaseStockAsync(int productVariantId, decimal quantity, CancellationToken cancellationToken = default);

        Task EnsureStockRecordExistsAsync(int productVariantId, CancellationToken cancellationToken = default);

        Task RecordMovementAsync(
                    int productVariantId,
                    Guid tenantId,
                    Guid branchId,
                    decimal quantityDelta,
                    StockMovementType type,
                    Guid referenceId,
                    string? reason = null,
                    string? notes = null,
                    CancellationToken cancellationToken = default);

        // ---- Batch API for multi-item handlers (CreateSale, CreatePurchase) ----

        Task<IReadOnlyDictionary<int, ProductVariant>> LoadVariantsAsync(
            IReadOnlyCollection<int> variantIds,
            CancellationToken cancellationToken = default);

        Task<StockContext> LoadStockContextAsync(
            IReadOnlyDictionary<int, ProductVariant> variants,
            Guid branchId,
            Guid tenantId,
            CancellationToken cancellationToken = default);

        StockMovement RecordMovement(
            StockContext context,
            int productVariantId,
            decimal quantityDelta,
            StockMovementType movementType,
            Guid referenceId,
            string? reason = null,
            string? notes = null);

        bool HasStockAvailable(StockContext context, int productVariantId, decimal requiredQuantity);

        decimal GetAvailable(StockContext context, int productVariantId);

        // ---- Batch ledger ----
        // Invariant: sum(open batches.RemainingQuantity) == Stock.AvailableQuantity per (variant, branch).

        StockBatch CreateBatch(
            StockContext context,
            int productVariantId,
            decimal quantity,
            decimal unitCost,
            Guid? purchaseItemId);

        // Returns empty when order == Single; caller falls back to Product.Cost for COGS.
        IReadOnlyList<BatchConsumption> ConsumeBatches(
            StockContext context,
            int productVariantId,
            decimal quantity,
            InventoryConsumptionOrder order);

        // Always FIFO regardless of tenant setting — adjustments only need invariant preservation.
        Task SyncBatchesOnAdjustmentAsync(
            int productVariantId,
            Guid branchId,
            Guid tenantId,
            decimal delta,
            CancellationToken cancellationToken = default);
    }
}

