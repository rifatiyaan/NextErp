using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface IMenuItemRepository : IRepositoryBase<MenuItem, int>
    {
        Task<IEnumerable<MenuItem>> GetMenuByUserRolesAsync(string[] roles, Guid tenantId);
        Task<IEnumerable<MenuItem>> GetMenuByModuleAsync(Guid moduleId);
        Task<MenuItem?> GetByUrlAsync(string url, Guid tenantId);
        Task<IEnumerable<MenuItem>> GetRootMenuItemsAsync(Guid tenantId);
        IQueryable<MenuItem> Query();
    }
}
