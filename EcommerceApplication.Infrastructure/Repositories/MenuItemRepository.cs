using EcommerceApplicationWeb.Domain.Entities;
using EcommerceApplicationWeb.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApplicationWeb.Infrastructure.Repositories
{
    public class MenuItemRepository : Repository<MenuItem, int>, IMenuItemRepository
    {
        public MenuItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MenuItem>> GetMenuByUserRolesAsync(string[] roles, Guid tenantId)
        {
            // Note: This is a simplified implementation. In a real scenario with JSON arrays,
            // you might need a more complex query or client-side filtering if EF Core translation is limited.
            // For now, we'll fetch active items and filter in memory for roles.
            
            var allItems = await _dbSet
                .Where(x => x.TenantId == tenantId && x.IsActive)
                .Include(x => x.Children)
                .OrderBy(x => x.Order)
                .ToListAsync();

            // Filter by roles (if roles are defined on the item)
            // If item has no roles defined, it's visible to everyone (or no one, depending on policy)
            // Here assuming: No roles = visible to all.
            
            return allItems.Where(item => 
                item.Metadata.Roles == null || 
                !item.Metadata.Roles.Any() || 
                item.Metadata.Roles.Intersect(roles).Any()
            ).ToList();
        }

        public async Task<IEnumerable<MenuItem>> GetMenuByModuleAsync(Guid moduleId)
        {
            return await _dbSet
                .Where(x => x.ModuleId == moduleId)
                .ToListAsync();
        }

        public async Task<MenuItem?> GetByUrlAsync(string url, Guid tenantId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Url == url && x.TenantId == tenantId);
        }

        public async Task<IEnumerable<MenuItem>> GetRootMenuItemsAsync(Guid tenantId)
        {
            return await _dbSet
                .Where(x => x.ParentId == null && x.TenantId == tenantId)
                .Include(x => x.Children)
                .OrderBy(x => x.Order)
                .ToListAsync();
        }

        public IQueryable<MenuItem> Query()
        {
            return _dbSet.AsQueryable();
        }
    }
}
