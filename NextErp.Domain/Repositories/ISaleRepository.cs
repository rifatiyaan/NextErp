using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface ISaleRepository : IRepositoryBase<Sale, Guid>
    {
        Task<(IList<Sale> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy);

        Task<Sale?> GetByIdWithDetailsAsync(Guid id);

        Task<IList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        IQueryable<Sale> Query();
    }
}
