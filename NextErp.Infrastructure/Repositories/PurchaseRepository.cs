using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;
using System.Linq.Expressions;

namespace NextErp.Infrastructure.Repositories;

public class PurchaseRepository(IApplicationDbContext applicationContext, IBranchProvider branchProvider)
    : Repository<Purchase, Guid>((DbContext)applicationContext), IPurchaseRepository
{
    private readonly DbContext _db = (DbContext)applicationContext;

    public async Task<(IList<Purchase> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex,
            int pageSize,
            string? searchText,
            string? orderBy,
            IReadOnlyList<int>? supplierIds = null,
            bool? isActiveFilter = null)
    {
        _ = supplierIds;

        var nonGlobal = !branchProvider.IsGlobal();
        var branchId = nonGlobal ? branchProvider.GetRequiredBranchId() : (Guid?)null;
        var root = CreatePurchaseRootQuery(nonGlobal, isActiveFilter);

        var filter = BuildRowFilter(searchText, nonGlobal, branchId, isActiveFilter);
        Func<IQueryable<Purchase>, IIncludableQueryable<Purchase, object>> include = BuildPurchaseIncludes();

        return await GetDynamicFromQueryAsync(root, filter, orderBy, include, pageIndex, pageSize, true);
    }

    private IQueryable<Purchase> CreatePurchaseRootQuery(bool nonGlobal, bool? isActiveFilter)
    {
        var query = _db.Set<Purchase>().AsQueryable();
        var needUnfiltered = nonGlobal && (isActiveFilter == null || isActiveFilter == false);
        return needUnfiltered ? query.IgnoreQueryFilters() : query;
    }

    private static Expression<Func<Purchase, bool>> BuildRowFilter(
        string? searchText,
        bool nonGlobal,
        Guid? branchId,
        bool? isActiveFilter)
    {
        // Keep predicate as a single expression so EF Core can translate it to SQL.
        return x =>
            (string.IsNullOrEmpty(searchText)
             || x.Title.Contains(searchText)
             || x.PurchaseNumber.Contains(searchText)
             || (x.Party != null && x.Party.Title.Contains(searchText)))
            && (!nonGlobal || x.BranchId == branchId!.Value)
            && (isActiveFilter == null || (isActiveFilter.Value ? x.IsActive : !x.IsActive));
    }

    private static Func<IQueryable<Purchase>, IIncludableQueryable<Purchase, object>> BuildPurchaseIncludes() =>
        q => q.Include(p => p.Party)
            .Include(p => p.Items)
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .ThenInclude(pr => pr.Category);

    public async Task<Purchase?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _db.Set<Purchase>()
            .AsNoTracking()
            .Include(p => p.Party)
            .Include(p => p.Items)
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .ThenInclude(pr => pr.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IList<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _db.Set<Purchase>()
            .AsNoTracking()
            .Include(p => p.Party)
            .Include(p => p.Items)
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .ThenInclude(pr => pr.Category)
            .Where(p => p.PurchaseDate >= startDate && p.PurchaseDate <= endDate)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }

    public IQueryable<Purchase> Query() => _db.Set<Purchase>().AsQueryable();
}
