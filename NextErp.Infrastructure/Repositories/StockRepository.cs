using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;

namespace NextErp.Infrastructure.Repositories;

public class StockRepository(IApplicationDbContext applicationContext, IBranchProvider branchProvider)
    : Repository<Stock, Guid>((DbContext)applicationContext), IStockRepository
{
    private readonly DbContext _db = (DbContext)applicationContext;

    public async Task<Stock?> GetByProductVariantIdAsync(int productVariantId, CancellationToken cancellationToken = default)
    {
        IQueryable<Stock> query = _db.Set<Stock>()
            .Include(s => s.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .Where(s => s.ProductVariantId == productVariantId);

        if (!branchProvider.IsGlobal())
            query = query.Where(s => s.BranchId == branchProvider.GetRequiredBranchId());

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Stock?> GetByProductVariantIdAndBranchIdAsync(
        int productVariantId,
        Guid branchId,
        CancellationToken cancellationToken = default) =>
        _db.Set<Stock>()
            .Include(s => s.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(
                s => s.ProductVariantId == productVariantId && s.BranchId == branchId,
                cancellationToken);

    public async Task<IList<Stock>> GetAllWithVariantsAsync() =>
        await _db.Set<Stock>()
            .Include(s => s.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .ThenInclude(p => p.Category)
            .Where(s => s.AvailableQuantity >= 0)
            .ToListAsync();

    public async Task<IList<Stock>> GetLowStockAsync() =>
        await _db.Set<Stock>()
            .Include(s => s.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .Where(s => s.AvailableQuantity <= 10)
            .ToListAsync();

    public IQueryable<Stock> Query() => _db.Set<Stock>().AsQueryable();

    public async Task<IReadOnlyList<(int ProductId, decimal TotalAvailable, bool HasLowStock)>>
        GetProductStockAggregatesAsync(
            IReadOnlyList<int> productIds,
            CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return Array.Empty<(int, decimal, bool)>();

        const decimal lowThreshold = 10m;

        var rows = await _db.Set<ProductVariant>()
            .AsNoTracking()
            .Where(v => productIds.Contains(v.ProductId))
            .GroupBy(v => v.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalAvailable = g.SelectMany(v => v.StockRecords).Select(s => (decimal?)s.AvailableQuantity).Sum() ?? 0m,
                HasLowStock = g.SelectMany(v => v.StockRecords).Any(s => s.AvailableQuantity <= lowThreshold),
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r => (r.ProductId, r.TotalAvailable, r.HasLowStock)).ToList();
    }
}
