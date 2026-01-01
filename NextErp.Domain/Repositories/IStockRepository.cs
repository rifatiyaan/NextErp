using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface IStockRepository : IRepositoryBase<Stock, int>
    {
        /// <summary>
        /// Get stock by product ID
        /// </summary>
        Task<Stock?> GetByProductIdAsync(int productId);

        /// <summary>
        /// Get all stocks with product information
        /// </summary>
        Task<IList<Stock>> GetAllWithProductsAsync();

        /// <summary>
        /// Get low stock items (where AvailableQuantity <= Product.ReorderLevel)
        /// Note: ReorderLevel would need to be added to Product entity if not exists
        /// </summary>
        Task<IList<Stock>> GetLowStockAsync();

        IQueryable<Stock> Query();
    }
}
