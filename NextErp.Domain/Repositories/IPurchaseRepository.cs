using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface IPurchaseRepository : IRepositoryBase<Purchase, Guid>
    {
        Task<(IList<Purchase> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy);

        Task<Purchase?> GetByIdWithDetailsAsync(Guid id);

        Task<IList<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        IQueryable<Purchase> Query();
    }
}
