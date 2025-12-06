using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Infrastructure.Repositories
{
    public class ModuleRepository : Repository<Module, int>, IModuleRepository
    {
        public ModuleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Module>> GetMenuByUserRolesAsync(string[] roles, Guid tenantId)
        {
            // Get all active menu links (Type = Link) for the tenant
            var allItems = await _dbSet
                .Where(x => x.TenantId == tenantId && x.IsActive && x.Type == ModuleType.Link)
                .Include(x => x.Children)
                .OrderBy(x => x.Order)
                .ToListAsync();

            // Filter by roles (if roles are defined on the item)
            // If item has no roles defined, it's visible to everyone
            return allItems.Where(item => 
                item.Metadata.Roles == null || 
                !item.Metadata.Roles.Any() || 
                item.Metadata.Roles.Intersect(roles).Any()
            ).ToList();
        }

        public async Task<IEnumerable<Module>> GetModulesAsync(Guid tenantId)
        {
            return await _dbSet
                .Where(x => x.TenantId == tenantId && x.Type == ModuleType.Module)
                .Include(x => x.Children)
                .OrderBy(x => x.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<Module>> GetMenuLinksAsync(Guid tenantId)
        {
            return await _dbSet
                .Where(x => x.TenantId == tenantId && x.Type == ModuleType.Link)
                .Include(x => x.Children)
                .OrderBy(x => x.Order)
                .ToListAsync();
        }

        public async Task<Module?> GetByUrlAsync(string url, Guid tenantId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Url == url && x.TenantId == tenantId);
        }

        public async Task<IEnumerable<Module>> GetRootItemsAsync(Guid tenantId)
        {
            return await _dbSet
                .Where(x => x.ParentId == null && x.TenantId == tenantId)
                .Include(x => x.Children)
                .OrderBy(x => x.Order)
                .ToListAsync();
        }

        public IQueryable<Module> Query()
        {
            return _dbSet.AsQueryable();
        }
    }
}
