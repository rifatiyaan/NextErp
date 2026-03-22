namespace NextErp.Application.Interfaces
{
    public interface IStockService
    {
        Task<bool> CheckStockAvailabilityAsync(int productVariantId, decimal requiredQuantity, CancellationToken cancellationToken = default);

        Task<decimal> GetAvailableStockAsync(int productVariantId, CancellationToken cancellationToken = default);

        Task ReduceStockAsync(int productVariantId, decimal quantity, CancellationToken cancellationToken = default);

        Task IncreaseStockAsync(int productVariantId, decimal quantity, CancellationToken cancellationToken = default);

        Task EnsureStockRecordExistsAsync(int productVariantId, Guid tenantId, CancellationToken cancellationToken = default);
    }
}
