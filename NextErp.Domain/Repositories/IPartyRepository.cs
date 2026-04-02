using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface IPartyRepository : IRepositoryBase<Party, Guid>
    {
        Task<(IList<Party> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy, PartyType? partyType = null);

        IQueryable<Party> Query();
    }
}
