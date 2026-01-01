using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface ICustomerRepository : IRepositoryBase<Customer, Guid>
    {
        Task<(IList<Customer> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy);

        IQueryable<Customer> Query();
    }
}
