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

        public async Task<Stock?> GetByProductIdAsync(int productId)
        {
            return await _db.Set<Stock>()
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.ProductId == productId);
        }

        public async Task<IList<Stock>> GetAllWithProductsAsync()
        {
            return await _db.Set<Stock>()
                .Include(s => s.Product)
                .Where(s => s.AvailableQuantity >= 0)
                .ToListAsync();
        }

        public async Task<IList<Stock>> GetLowStockAsync()
        {
            // Returns stocks with quantity <= 10 (you can adjust this threshold)
            return await _db.Set<Stock>()
                .Include(s => s.Product)
                .Where(s => s.AvailableQuantity <= 10)
                .ToListAsync();
        }

        public IQueryable<Stock> Query()
        {
            return _db.Set<Stock>().AsQueryable();
        }
    }
}
