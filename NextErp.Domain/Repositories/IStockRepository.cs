using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
public interface IStockRepository : IRepositoryBase<Stock, Guid>
    {
        Task<Stock?> GetByProductVariantIdAsync(int productVariantId, CancellationToken cancellationToken = default);

        Task<IList<Stock>> GetAllWithVariantsAsync();

        Task<IList<Stock>> GetLowStockAsync();

        /// <summary>
        /// One row per product id: sum of variant ledger quantities and whether any variant is low (≤10).
        /// Single grouped query; no N+1.
        /// </summary>
        Task<IReadOnlyList<(int ProductId, decimal TotalAvailable, bool HasLowStock)>> GetProductStockAggregatesAsync(
            IReadOnlyList<int> productIds,
            CancellationToken cancellationToken = default);

        IQueryable<Stock> Query();
    }
}
