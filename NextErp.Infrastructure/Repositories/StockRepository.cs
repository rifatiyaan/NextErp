using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Infrastructure.Repositories
{
    public class StockRepository : Repository<Stock, int>, IStockRepository
    {
        private readonly DbContext _db;

        public StockRepository(IApplicationDbContext context) : base((DbContext)context)
        {
            _db = (DbContext)context;
        }

        public async Task<Stock?> GetByProductVariantIdAsync(int productVariantId)
        {
            var tracked = _db.ChangeTracker.Entries<Stock>()
                .FirstOrDefault(e => e.Entity.Id == productVariantId);

            if (tracked != null)
                return tracked.Entity;

            return await _db.Set<Stock>()
                .Include(s => s.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(s => s.Id == productVariantId);
        }

        public async Task<IList<Stock>> GetAllWithVariantsAsync()
        {
            return await _db.Set<Stock>()
                .Include(s => s.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                        .ThenInclude(p => p.Category)
                .Where(s => s.AvailableQuantity >= 0)
                .ToListAsync();
        }

        public async Task<IList<Stock>> GetLowStockAsync()
        {
            return await _db.Set<Stock>()
                .Include(s => s.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Where(s => s.AvailableQuantity <= 10)
                .ToListAsync();
        }

        public IQueryable<Stock> Query()
        {
            return _db.Set<Stock>().AsQueryable();
        }
    }
}
