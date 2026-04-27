using NextErp.Domain.Entities;

namespace NextErp.Application.Interfaces
{
    public interface IStockService
    {
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
    }
}
