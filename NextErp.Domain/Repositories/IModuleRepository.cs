using NextErp.Domain.Entities;

namespace NextErp.Domain.Repositories
{
    public interface IModuleRepository : IRepositoryBase<Module, int>
    {
        Task<IEnumerable<Module>> GetMenuByUserRolesAsync(string[] roles, Guid tenantId);
        Task<IEnumerable<Module>> GetModulesAsync(Guid tenantId);
        Task<IEnumerable<Module>> GetMenuLinksAsync(Guid tenantId);
        Task<Module?> GetByUrlAsync(string url, Guid tenantId);
        Task<IEnumerable<Module>> GetRootItemsAsync(Guid tenantId);
        IQueryable<Module> Query();
    }
}
