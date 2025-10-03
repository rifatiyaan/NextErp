
using EcommerceApplicationWeb.Domain.Entities;

namespace EcommerceApplicationWeb.Domain.Repositories
{
    public interface IProductRepository : IRepositoryBase<Product, int>
    {
        Task<(IList<Product> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? orderBy, int? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null);

        Task<(IList<Product> records, int total, int totalDisplay)> GetTableDataAsync(
            int pageIndex, int pageSize, string? searchText, string? sortBy, Dictionary<string, object?> filters);

        IQueryable<Product> Query();
    }

}
