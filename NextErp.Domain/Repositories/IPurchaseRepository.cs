using System.Collections.Generic;
using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface IPurchaseRepository : IRepositoryBase<Purchase, Guid>
    {
        Task<(IList<Purchase> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex,
            int pageSize,
            string? searchText,
            string? orderBy,
            IReadOnlyList<int>? supplierIds = null,
            bool? isActiveFilter = null);
        // supplierIds parameter kept for signature compatibility (no longer filtered)

        Task<Purchase?> GetByIdWithDetailsAsync(Guid id);

        Task<IList<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        IQueryable<Purchase> Query();
    }
}
