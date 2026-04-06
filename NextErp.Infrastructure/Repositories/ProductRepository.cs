using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;

namespace NextErp.Infrastructure.Repositories;

public class ProductRepository(IApplicationDbContext context)
    : Repository<Product, int>((DbContext)context), IProductRepository
{
    private readonly DbSet<Product> _products = ((DbContext)context).Set<Product>();

    public IQueryable<Product> Query() => _products.AsQueryable();

    public Task<(IList<Product> records, int total, int totalDisplay)> GetTableDataAsync(
        int pageIndex,
        int pageSize,
        string? searchText,
        string? sortBy,
        Dictionary<string, object?> filters) =>
        throw new NotImplementedException();

    public Task<(IList<Product> records, int total, int totalDisplay)> GetTableDataAsync(
        int pageIndex,
        int pageSize,
        string? searchText,
        string? orderBy,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null) =>
        throw new NotImplementedException();
}
