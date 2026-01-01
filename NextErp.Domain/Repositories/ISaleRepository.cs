using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface ISaleRepository : IRepositoryBase<Sale, Guid>
    {
        Task<(IList<Sale> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy);

        /// <summary>
        /// Get sale with items and related entities
        /// </summary>
        Task<Sale?> GetByIdWithDetailsAsync(Guid id);

        /// <summary>
        /// Get sales within date range for reporting
        /// </summary>
        Task<IList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        IQueryable<Sale> Query();
    }
}
