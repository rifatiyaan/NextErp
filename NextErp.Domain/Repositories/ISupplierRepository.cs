using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface ISupplierRepository : IRepositoryBase<Supplier, int>
    {
        Task<(IList<Supplier> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy);

        IQueryable<Supplier> Query();
    }
}
