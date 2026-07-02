using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Common;
using NextErp.Application.Common.Extensions;
using NextErp.Application.DTOs.Product;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using NextErp.Application.Products;
using NextErp.Application.Queries;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Product;

public class GetPagedProductsHandler(
    IApplicationDbContext dbContext,
    IBranchProvider branchProvider)
    : IRequestHandler<GetPagedProductsQuery, PagedResult<ProductResponse>>
{
    public async Task<PagedResult<ProductResponse>> Handle(
        GetPagedProductsQuery request,
        CancellationToken cancellationToken = default)
    {
        var pattern = !string.IsNullOrWhiteSpace(request.SearchText)
            ? $"%{request.SearchText.Trim()}%"
            : null;

        var query = ApplyStatusFilter(dbContext.Products.AsQueryable(), request.Status)
            .WhereIfNotEmpty(pattern, p =>
                EF.Functions.Like(p.Title, pattern!) ||
                EF.Functions.Like(p.Code, pattern!))
            .WhereIf(request.CategoryId is > 0, p => p.CategoryId == request.CategoryId!.Value);

        var total = await query.CountAsync(cancellationToken);

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "title" => query.OrderBy(p => p.Title),
            "price" => query.OrderBy(p => p.Price),
            _ => query.OrderByDescending(p => p.CreatedAt),
        };

        var records = await query
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.UnitOfMeasure)
            .Include(p => p.ProductVariants)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = records.Select(r => r.ToResponse()).ToList();

        if (dtos.Count > 0)
        {
            await ProductVariantStockLookup.EnrichProductListVariantStocksAsync(dtos, dbContext, branchProvider, cancellationToken)
                .ConfigureAwait(false);

            // Populate TotalAvailableQuantity + HasLowStock as default list behaviour
            // (previously gated behind includeStock=true).
            ApplyStockColumns(dtos, await LoadStockLookupAsync(records, cancellationToken));
        }

        return new PagedResult<ProductResponse>(dtos, total, total);
    }

    private IQueryable<Entities.Product> ApplyStatusFilter(IQueryable<Entities.Product> query, string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return branchProvider.IsGlobal() ? query.Where(p => p.IsActive) : query;

        return status.ToLowerInvariant() switch
        {
            "closed" => ApplyInactiveProductsForCurrentScope(query),
            "active" => branchProvider.IsGlobal() ? query.Where(p => p.IsActive) : query,
            "out of stock" => ApplyOutOfStock(query),
            _ => branchProvider.IsGlobal() ? query.Where(p => p.IsActive) : query,
        };
    }

    private IQueryable<Entities.Product> ApplyInactiveProductsForCurrentScope(IQueryable<Entities.Product> query)
    {
        if (branchProvider.IsGlobal())
            return query.Where(p => !p.IsActive);

        var branchId = branchProvider.GetRequiredBranchId();
        return query.IgnoreQueryFilters().Where(p => p.BranchId == branchId && !p.IsActive);
    }

    private IQueryable<Entities.Product> ApplyOutOfStock(IQueryable<Entities.Product> query) =>
        branchProvider.IsGlobal()
            ? query.Where(p => p.IsActive && !p.ProductVariants.Any(v => v.StockRecords.Any(s => s.AvailableQuantity > 0)))
            : query.Where(p => !p.ProductVariants.Any(v => v.StockRecords.Any(s => s.AvailableQuantity > 0)));

    private async Task<IReadOnlyDictionary<int, (decimal TotalAvailable, bool HasLowStock)>> LoadStockLookupAsync(
        IReadOnlyList<Entities.Product> records,
        CancellationToken cancellationToken = default)
    {
        var ids = records.Select(p => p.Id).Distinct().ToArray();
        if (ids.Length == 0)
            return new Dictionary<int, (decimal, bool)>();

        // Inlined from former IStockRepository.GetProductStockAggregatesAsync.
        var branchId = branchProvider.GetBranchId();
        var rows = await dbContext.ProductVariants
            .AsNoTracking()
            .Where(v => ids.Contains(v.ProductId))
            .GroupBy(v => v.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalAvailable = g.SelectMany(v => v.StockRecords)
                    .Where(sr => !branchId.HasValue || sr.BranchId == branchId.Value)
                    .Select(s => (decimal?)s.AvailableQuantity)
                    .Sum() ?? 0m,
                HasLowStock = g.SelectMany(v => v.StockRecords)
                    .Where(sr => !branchId.HasValue || sr.BranchId == branchId.Value)
                    .Any(s => s.AvailableQuantity <= (s.ReorderLevel ?? 10m)),
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.ProductId, r => (r.TotalAvailable, r.HasLowStock));
    }

    private static void ApplyStockColumns(
        IReadOnlyList<ProductResponse> dtos,
        IReadOnlyDictionary<int, (decimal TotalAvailable, bool HasLowStock)> lookup)
    {
        foreach (var d in dtos)
        {
            if (!lookup.TryGetValue(d.Id, out var row))
            {
                d.TotalAvailableQuantity = 0m;
                d.HasLowStock = false;
                continue;
            }

            d.TotalAvailableQuantity = row.TotalAvailable;
            d.HasLowStock = row.HasLowStock;
        }
    }
}
