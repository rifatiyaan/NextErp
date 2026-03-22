using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface IStockRepository : IRepositoryBase<Stock, int>
    {
        Task<Stock?> GetByProductVariantIdAsync(int productVariantId);

        Task<IList<Stock>> GetAllWithVariantsAsync();

        Task<IList<Stock>> GetLowStockAsync();

        IQueryable<Stock> Query();
    }
}
