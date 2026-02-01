using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface IStockRepository : IRepositoryBase<Stock, int>
    {
        Task<Stock?> GetByProductIdAsync(int productId);

        Task<IList<Stock>> GetAllWithProductsAsync();

        Task<IList<Stock>> GetLowStockAsync();

        IQueryable<Stock> Query();
    }
}
