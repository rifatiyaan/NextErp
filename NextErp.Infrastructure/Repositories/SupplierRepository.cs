using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace NextErp.Infrastructure.Repositories
{
    public class SupplierRepository : Repository<Supplier, int>, ISupplierRepository
    {
        private readonly DbContext _db;

        public SupplierRepository(IApplicationDbContext context) : base((DbContext)context)
        {
            _db = (DbContext)context;
        }

        public async Task<(IList<Supplier> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy)
        {
            Expression<Func<Supplier, bool>> filter = x =>
                string.IsNullOrEmpty(searchText) || x.Title.Contains(searchText);

            return await GetDynamicAsync(filter, orderBy, null, pageIndex, pageSize, true);
        }

        public IQueryable<Supplier> Query()
        {
            return _db.Set<Supplier>().AsQueryable();
        }
    }
}
