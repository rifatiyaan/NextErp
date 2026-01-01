using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace NextErp.Infrastructure.Repositories
{
    public class PurchaseRepository : Repository<Purchase, Guid>, IPurchaseRepository
    {
        private readonly DbContext _db;

        public PurchaseRepository(IApplicationDbContext context) : base((DbContext)context)
        {
            _db = (DbContext)context;
        }

        public async Task<(IList<Purchase> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy)
        {
            Expression<Func<Purchase, bool>> filter = x =>
                string.IsNullOrEmpty(searchText) || 
                x.Title.Contains(searchText) || 
                x.PurchaseNumber.Contains(searchText);

            return await GetDynamicAsync(filter, orderBy, null, pageIndex, pageSize);
        }

        public async Task<Purchase?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _db.Set<Purchase>()
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IList<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _db.Set<Purchase>()
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product)
                .Where(p => p.PurchaseDate >= startDate && p.PurchaseDate <= endDate)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        public IQueryable<Purchase> Query()
        {
            return _db.Set<Purchase>().AsQueryable();
        }
    }
}
