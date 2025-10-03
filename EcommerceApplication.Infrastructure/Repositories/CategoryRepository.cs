using EcommerceApplicationWeb.Domain.Entities;
using EcommerceApplicationWeb.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EcommerceApplicationWeb.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category, int>, ICategoryRepository
    {
        private readonly DbContext _db;

        public CategoryRepository(IApplicationDbContext context) : base((DbContext)context)
        {
            _db = (DbContext)context;
        }

        public async Task<(IList<Category> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy)
        {
            Expression<Func<Category, bool>> filter = x =>
                string.IsNullOrEmpty(searchText) || x.Title.Contains(searchText);

            return await GetDynamicAsync(filter, orderBy, null, pageIndex, pageSize);
        }

        // Corrected Query method to return IQueryable<Category>
        public IQueryable<Category> Query()
        {
            return _db.Set<Category>().AsQueryable();
        }
    }
}
