using NextErp.Application.Services;
using NextErp.Domain.Entities;

namespace NextErp.Application.Interfaces
{
    public interface IStockService
    {
        // ---- Single-variant async API (kept for single-item handlers) ----

        Task<bool> CheckStockAvailabilityAsync(int productVariantId, decimal requiredQuantity, CancellationToken cancellationToken = default);

        Task<decimal> GetAvailableStockAsync(int productVariantId, CancellationToken cancellationToken = default);

        /// <summary>Sets on-hand quantity for the variant in the current branch (via adjustments). Does not create a row until a positive adjustment occurs.</summary>
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

        /// <summary>
        /// Loads ProductVariants (with Product include) for the given ids. One query.
        /// Throws <see cref="InvalidOperationException"/> if any id is not found.
        /// </summary>
        Task<IReadOnlyDictionary<int, ProductVariant>> LoadVariantsAsync(
            IReadOnlyCollection<int> variantIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Builds a <see cref="StockContext"/> from already-loaded variants and the existing
        /// Stock rows for the given branch. One query (stocks only). Variants that have no
        /// Stock row yet are absent from the context until <see cref="RecordMovement"/> creates one.
        /// </summary>
        Task<StockContext> LoadStockContextAsync(
            IReadOnlyDictionary<int, ProductVariant> variants,
            Guid branchId,
            Guid tenantId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronous in-memory mutation: stages a StockMovement on the DbContext, mutates
        /// the tracked Stock row, and creates a new Stock row if needed (positive delta only).
        /// No DB round-trip. Caller must call SaveChangesAsync to persist.
        /// </summary>
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
    }
}
