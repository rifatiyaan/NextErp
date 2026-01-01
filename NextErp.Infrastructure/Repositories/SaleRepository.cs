using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace NextErp.Infrastructure.Repositories
{
    public class SaleRepository : Repository<Sale, Guid>, ISaleRepository
    {
        private readonly DbContext _db;

        public SaleRepository(IApplicationDbContext context) : base((DbContext)context)
        {
            _db = (DbContext)context;
        }

        public async Task<(IList<Sale> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy)
        {
            Expression<Func<Sale, bool>> filter = x =>
                string.IsNullOrEmpty(searchText) || 
                x.Title.Contains(searchText) || 
                x.SaleNumber.Contains(searchText);

            return await GetDynamicAsync(filter, orderBy, null, pageIndex, pageSize);
        }

        public async Task<Sale?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _db.Set<Sale>()
                .Include(s => s.Customer)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _db.Set<Sale>()
                .Include(s => s.Customer)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public IQueryable<Sale> Query()
        {
            return _db.Set<Sale>().AsQueryable();
        }
    }
}
