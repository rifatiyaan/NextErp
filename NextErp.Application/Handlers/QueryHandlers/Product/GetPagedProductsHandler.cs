using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application;
using NextErp.Application.Common;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using Entities = NextErp.Domain.Entities;
using ProductDto = NextErp.Application.DTOs.Product;

namespace NextErp.Application.Handlers.QueryHandlers.Product;

public class GetPagedProductsHandler(
    IApplicationUnitOfWork unitOfWork,
    IBranchProvider branchProvider,
    IMapper mapper)
    : IRequestHandler<GetPagedProductsQuery, PagedResult<ProductDto.Response.Get.Single>>
{
    public async Task<PagedResult<ProductDto.Response.Get.Single>> Handle(
        GetPagedProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = ApplyStatusFilter(unitOfWork.ProductRepository.Query(), request.Status);

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var pattern = $"%{request.SearchText.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Title, pattern) ||
                EF.Functions.Like(p.Code, pattern));
        }

        if (request.CategoryId is > 0)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

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
            .Include(p => p.ProductVariants)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = mapper.Map<List<ProductDto.Response.Get.Single>>(records);

        if (request.IncludeStock && dtos.Count > 0)
            ApplyStockColumns(dtos, await LoadStockLookupAsync(records, cancellationToken));

        return new PagedResult<ProductDto.Response.Get.Single>(dtos, total, total);
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
            ? query.Where(p => p.IsActive && !p.ProductVariants.Any(v => v.Stock > 0))
            : query.Where(p => !p.ProductVariants.Any(v => v.Stock > 0));

    private async Task<IReadOnlyDictionary<int, (decimal TotalAvailable, bool HasLowStock)>> LoadStockLookupAsync(
        IReadOnlyList<Entities.Product> records,
        CancellationToken cancellationToken)
    {
        var ids = records.Select(p => p.Id).Distinct().ToArray();
        var rows = await unitOfWork.StockRepository.GetProductStockAggregatesAsync(ids, cancellationToken);
        return rows.ToDictionary(t => t.ProductId, t => (t.TotalAvailable, t.HasLowStock));
    }

    private static void ApplyStockColumns(
        List<ProductDto.Response.Get.Single> dtos,
        IReadOnlyDictionary<int, (decimal TotalAvailable, bool HasLowStock)> lookup)
    {
        foreach (var d in dtos)
        {
            if (lookup.TryGetValue(d.Id, out var row))
            {
                d.TotalAvailableQuantity = row.TotalAvailable;
                d.HasLowStock = row.HasLowStock;
            }
            else
            {
                d.TotalAvailableQuantity = 0m;
                d.HasLowStock = false;
            }
        }
    }
}
