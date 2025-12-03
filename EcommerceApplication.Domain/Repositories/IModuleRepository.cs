using EcommerceApplicationWeb.Domain.Entities;

namespace EcommerceApplicationWeb.Domain.Repositories
{
    public interface IModuleRepository : IRepositoryBase<Module, Guid>
    {
        Task<IEnumerable<Module>> GetInstalledModulesAsync(Guid tenantId);
        Task<IEnumerable<Module>> GetEnabledModulesAsync(Guid tenantId);
        Task<Module?> GetByNameAsync(string name, Guid tenantId);
        IQueryable<Module> Query();
    }
}
