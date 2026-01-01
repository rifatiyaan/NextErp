using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product, int>, IProductRepository
    {
        private readonly DbSet<Product> _products;

        public ProductRepository(IApplicationDbContext context) : base((DbContext)context)
        {
            _products = ((DbContext)context).Set<Product>();
        }

        public IQueryable<Product> Query() => _products.AsQueryable();

        //public async Task<(IList<Product> records, int total, int totalDisplay)> GetTableDataAsync(
        //    int pageIndex, int pageSize, string? searchText, string? orderBy, int? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null)
        //{
        //    var query = _products
        //        .AsQueryable()
        //        .WhereWhen(!string.IsNullOrEmpty(searchText), p => p.Title.Contains(searchText))
        //        .WhereWhen(categoryId.HasValue, p => p.CategoryId == categoryId.Value)
        //        .WhereWhen(minPrice.HasValue, p => p.Price >= minPrice.Value)
        //        .WhereWhen(maxPrice.HasValue, p => p.Price <= maxPrice.Value)
        //        .IncludeWhen(true, p => p.Category)
        //        .IncludeWhen(true, p => p.Parent)
        //        .IncludeWhen(true, p => p.Children); // children included separately

        //    return await GetDynamicAsync(query, orderBy, null, pageIndex, pageSize);
        //}

        public Task<(IList<Product> records, int total, int totalDisplay)> GetTableDataAsync(int pageIndex, int pageSize, string? searchText, string? sortBy, Dictionary<string, object?> filters)
        {
            throw new NotImplementedException();
        }

        public Task<(IList<Product> records, int total, int totalDisplay)> GetTableDataAsync(int pageIndex, int pageSize, string? searchText, string? orderBy, int? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            throw new NotImplementedException();
        }

        //public Task<(IList<Product> records, int total, int totalDisplay)> GetTableDataAsync(
        //    int pageIndex, int pageSize, string? searchText, string? sortBy, Dictionary<string, object?> filters)
        //{
        //    var query = _products.AsQueryable();

        //    if (filters != null)
        //    {
        //        if (filters.TryGetValue("CategoryId", out var catId) && catId is int cid)
        //            query = query.Where(p => p.CategoryId == cid);

        //        if (filters.TryGetValue("MinPrice", out var min) && min is decimal minPrice)
        //            query = query.Where(p => p.Price >= minPrice);

        //        if (filters.TryGetValue("MaxPrice", out var max) && max is decimal maxPrice)
        //            query = query.Where(p => p.Price <= maxPrice);
        //    }

        //    query = query
        //        .WhereWhen(!string.IsNullOrEmpty(searchText), p => p.Title.Contains(searchText))
        //        .IncludeWhen(true, p => p.Category)
        //        .IncludeWhen(true, p => p.Parent)
        //        .IncludeWhen(true, p => p.Children); // children included separately

        //    return GetDynamicAsync(query, sortBy, null, pageIndex, pageSize);
        //}
    }
}
