using NextErp.Domain.Entities;

namespace NextErp.Application.Interfaces
{
    public interface IStockService
    {
        Task<bool> CheckStockAvailabilityAsync(int productVariantId, decimal requiredQuantity, CancellationToken cancellationToken = default);

        Task<decimal> GetAvailableStockAsync(int productVariantId, CancellationToken cancellationToken = default);

        Task ReduceStockAsync(int productVariantId, decimal quantity, CancellationToken cancellationToken = default);

        Task IncreaseStockAsync(int productVariantId, decimal quantity, CancellationToken cancellationToken = default);

        Task EnsureStockRecordExistsAsync(int productVariantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists a stock movement and updates <see cref="Stock.AvailableQuantity"/> for the variant and branch in the current unit of work.
        /// </summary>
        Task RecordMovementAsync(
            int productVariantId,
            Guid tenantId,
            Guid branchId,
            decimal quantityDelta,
            StockMovementType type,
            Guid referenceId,
            CancellationToken cancellationToken = default);
    }
}
