using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface ICategoryRepository : IRepositoryBase<Category, int>
    {
        Task<(IList<Category> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy);

        IQueryable<Category> Query();
    }
}
