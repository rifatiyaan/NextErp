using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface IStockRepository : IRepositoryBase<Stock, Guid>
    {
        Task<Stock?> GetByProductVariantIdAsync(int productVariantId, CancellationToken cancellationToken = default);

        Task<Stock?> GetByProductVariantIdAndBranchIdAsync(
            int productVariantId,
            Guid branchId,
            CancellationToken cancellationToken = default);

        Task<IList<Stock>> GetAllWithVariantsAsync();

        Task<IList<Stock>> GetLowStockAsync();

        Task<IReadOnlyList<(int ProductId, decimal TotalAvailable, bool HasLowStock)>> GetProductStockAggregatesAsync(
                    IReadOnlyList<int> productIds,
                    CancellationToken cancellationToken = default);

        IQueryable<Stock> Query();
    }
}
