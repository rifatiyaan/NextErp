using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace NextErp.Infrastructure.Repositories
{
    public class PartyRepository : Repository<Party, Guid>, IPartyRepository
    {
        private readonly DbContext _db;

        public PartyRepository(IApplicationDbContext context) : base((DbContext)context)
        {
            _db = (DbContext)context;
        }

        public async Task<(IList<Party> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy, PartyType? partyType = null)
        {
            Expression<Func<Party, bool>> filter = x =>
                (partyType == null || x.PartyType == partyType) &&
                (string.IsNullOrEmpty(searchText) ||
                 x.Title.Contains(searchText) ||
                 (x.Email != null && x.Email.Contains(searchText)) ||
                 (x.Phone != null && x.Phone.Contains(searchText)));

            return await GetDynamicAsync(filter, orderBy, null, pageIndex, pageSize, true);
        }

        public IQueryable<Party> Query()
        {
            return _db.Set<Party>().AsQueryable();
        }
    }
}
