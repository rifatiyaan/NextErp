using EcommerceApplicationWeb.Domain.Entities;
using EcommerceApplicationWeb.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApplicationWeb.Infrastructure.Repositories
{
    public class ModuleRepository : Repository<Module, Guid>, IModuleRepository
    {
        public ModuleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Module>> GetInstalledModulesAsync(Guid tenantId)
        {
            return await _dbSet
                .Where(x => x.TenantId == tenantId && x.IsInstalled)
                .ToListAsync();
        }

        public async Task<IEnumerable<Module>> GetEnabledModulesAsync(Guid tenantId)
        {
            return await _dbSet
                .Where(x => x.TenantId == tenantId && x.IsInstalled && x.IsEnabled)
                .ToListAsync();
        }

        public async Task<Module?> GetByTitleAsync(string title, Guid tenantId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Title == title && x.TenantId == tenantId);
        }

        public IQueryable<Module> Query()
        {
            return _dbSet.AsQueryable();
        }

        public Task<Module?> GetByNameAsync(string name, Guid tenantId)
        {
            throw new NotImplementedException();
        }
    }
}
