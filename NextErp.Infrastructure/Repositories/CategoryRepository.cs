using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace NextErp.Infrastructure.Repositories
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
                x.IsActive &&
                (string.IsNullOrEmpty(searchText) || x.Title.Contains(searchText));

            Func<IQueryable<Category>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Category, object>> include = q =>
                q.Include(c => c.Parent)
                 .Include(c => c.Children);

            return await GetDynamicAsync(filter, orderBy, include, pageIndex, pageSize, true);
        }

        // Corrected Query method to return IQueryable<Category>
        public IQueryable<Category> Query()
        {
            return _db.Set<Category>().AsQueryable();
        }
    }
}
