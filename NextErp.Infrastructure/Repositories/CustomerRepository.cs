using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace NextErp.Infrastructure.Repositories
{
    public class CustomerRepository : Repository<Customer, Guid>, ICustomerRepository
    {
        private readonly DbContext _db;

        public CustomerRepository(IApplicationDbContext context) : base((DbContext)context)
        {
            _db = (DbContext)context;
        }

        public async Task<(IList<Customer> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy)
        {
            Expression<Func<Customer, bool>> filter = x =>
                string.IsNullOrEmpty(searchText) || x.Title.Contains(searchText);

            return await GetDynamicAsync(filter, orderBy, null, pageIndex, pageSize);
        }

        public IQueryable<Customer> Query()
        {
            return _db.Set<Customer>().AsQueryable();
        }
    }
}
