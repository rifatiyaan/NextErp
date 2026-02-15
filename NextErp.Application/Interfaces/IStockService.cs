namespace NextErp.Application.Interfaces
{
    public interface IStockService
    {
        Task<bool> CheckStockAvailabilityAsync(int productId, decimal requiredQuantity, CancellationToken cancellationToken = default);
        Task<decimal> GetAvailableStockAsync(int productId, CancellationToken cancellationToken = default);
        Task ReduceStockAsync(int productId, decimal quantity, CancellationToken cancellationToken = default);
        Task IncreaseStockAsync(int productId, decimal quantity, CancellationToken cancellationToken = default);
        Task EnsureStockRecordExistsAsync(int productId, Guid tenantId, CancellationToken cancellationToken = default);
    }
}

