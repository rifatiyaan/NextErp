using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Purchase
{
    public class GetPagedPurchasesHandler(
        IApplicationDbContext dbContext,
        IBranchProvider branchProvider)
        : IRequestHandler<GetPagedPurchasesQuery, (IList<Entities.Purchase> Records, int Total, int TotalDisplay)>
    {
        public async Task<(IList<Entities.Purchase> Records, int Total, int TotalDisplay)> Handle(
            GetPagedPurchasesQuery request,
            CancellationToken cancellationToken = default)
        {
            // Inlined from former IPurchaseRepository.GetTableDataAsync.
            // supplierIds is intentionally unused (kept on the query record for signature compat).
            _ = request.SupplierIds;

            var nonGlobal = !branchProvider.IsGlobal();
            var branchId = nonGlobal ? branchProvider.GetRequiredBranchId() : (Guid?)null;
            var isActiveFilter = request.IsActiveFilter;

            // For non-global users wanting closed/inactive rows we must bypass the IsActive query filter.
            var needUnfiltered = nonGlobal && (isActiveFilter == null || isActiveFilter == false);
            var root = needUnfiltered
                ? dbContext.Purchases.IgnoreQueryFilters()
                : dbContext.Purchases.AsQueryable();

            var searchText = request.SearchText;
            var query = root.Where(x =>
                (string.IsNullOrEmpty(searchText)
                 || x.Title.Contains(searchText)
                 || x.PurchaseNumber.Contains(searchText)
                 || (x.Party != null && x.Party.Title.Contains(searchText)))
                && (!nonGlobal || x.BranchId == branchId!.Value)
                && (isActiveFilter == null || (isActiveFilter.Value ? x.IsActive : !x.IsActive)));

            var total = await query.CountAsync(cancellationToken);

            query = query
                .Include(p => p.Party)
                .Include(p => p.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(pr => pr.Category)
                .AsNoTracking();

            // Preserve original dynamic-string sort behaviour via System.Linq.Dynamic.Core.
            var ordered = string.IsNullOrWhiteSpace(request.SortBy)
                ? query
                : query.OrderBy(request.SortBy);

            var records = await ordered
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return (records, total, total);
        }
    }
}
