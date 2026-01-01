using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface IPurchaseRepository : IRepositoryBase<Purchase, Guid>
    {
        Task<(IList<Purchase> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy);

        /// <summary>
        /// Get purchase with items and related entities
        /// </summary>
        Task<Purchase?> GetByIdWithDetailsAsync(Guid id);

        /// <summary>
        /// Get purchases within date range for reporting
        /// </summary>
        Task<IList<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        IQueryable<Purchase> Query();
    }
}
