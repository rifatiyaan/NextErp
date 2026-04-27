using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using ProductGetSingle = NextErp.Application.DTOs.Product.Response.Get;

namespace NextErp.Application.Products;

/// <summary>
/// Stock projection for product DTOs: batched reads from <see cref="Domain.Entities.Stock"/> (single source of truth).
/// </summary>
public static class ProductVariantStockLookup
{
    /// <summary>
    /// One query: total <see cref="Domain.Entities.Stock.AvailableQuantity"/> per product,
    /// counting only rows whose <c>BranchId</c> matches the product catalog row (Products.BranchId).
    /// </summary>
    public static async Task<IReadOnlyDictionary<int, decimal>> GetProductAggregateStockTotalsAsync(
        IApplicationDbContext db,
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return new Dictionary<int, decimal>();

        var idArray = productIds.Distinct().ToArray();

        var sums = await (
                from s in db.Stocks.AsNoTracking()
                join v in db.ProductVariants.AsNoTracking() on s.ProductVariantId equals v.Id
                join p in db.Products.AsNoTracking() on v.ProductId equals p.Id
                where idArray.Contains(p.Id) && s.BranchId == p.BranchId
                group s by p.Id
                into g
                select new { ProductId = g.Key, Total = g.Sum(x => x.AvailableQuantity) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = idArray.ToDictionary(id => id, _ => 0m);
        foreach (var row in sums)
            result[row.ProductId] = row.Total;

        return result;
    }

    /// <summary>Total on-hand for one product (same rules as <see cref="GetProductAggregateStockTotalsAsync"/>).</summary>
    public static async Task<decimal> GetProductAggregateStockTotalAsync(
        int productId,
        IApplicationDbContext db,
        CancellationToken cancellationToken = default)
    {
        var map = await GetProductAggregateStockTotalsAsync(db, new[] { productId }, cancellationToken)
            .ConfigureAwait(false);
        return map.GetValueOrDefault(productId, 0m);
    }

    /// <summary>
    /// Sums Stock.AvailableQuantity per variant.
    /// When <see cref="IBranchProvider.GetBranchId"/> is set, restricts to that branch.
    /// When global with no branch claim, aggregates across all branches.
    /// </summary>
    public static async Task<IReadOnlyDictionary<int, decimal>> GetAvailableByVariantIdsAsync(
        IApplicationDbContext db,
        IBranchProvider branchProvider,
        IEnumerable<int> variantIds,
        CancellationToken cancellationToken = default)
    {
        var ids = variantIds.Distinct().ToArray();
        if (ids.Length == 0)
            return new Dictionary<int, decimal>();

        IQueryable<NextErp.Domain.Entities.Stock> q = db.Stocks.AsNoTracking()
            .Where(s => ids.Contains(s.ProductVariantId));

        if (branchProvider.GetBranchId() is { } branchId)
            q = q.Where(s => s.BranchId == branchId);

        var rows = await q
            .GroupBy(s => s.ProductVariantId)
            .Select(g => new { VariantId = g.Key, Qty = g.Sum(x => x.AvailableQuantity) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.ToDictionary(x => x.VariantId, x => x.Qty);
    }

    public static async Task EnrichProductVariantStocksAsync(
        ProductGetSingle.Single? dto,
        IApplicationDbContext db,
        IBranchProvider branchProvider,
        CancellationToken cancellationToken = default)
    {
        if (dto?.ProductVariants is not { Count: > 0 })
            return;

        var lookup = await GetAvailableByVariantIdsAsync(
                db,
                branchProvider,
                dto.ProductVariants.Select(v => v.Id),
                cancellationToken)
            .ConfigureAwait(false);

        ApplyVariantQuantities(dto.ProductVariants, lookup);
    }

    public static async Task EnrichProductListVariantStocksAsync(
        IReadOnlyList<ProductGetSingle.Single> dtos,
        IApplicationDbContext db,
        IBranchProvider branchProvider,
        CancellationToken cancellationToken = default)
    {
        if (dtos.Count == 0)
            return;

        var variantIds = dtos
            .Where(d => d.ProductVariants is { Count: > 0 })
            .SelectMany(d => d.ProductVariants!)
            .Select(v => v.Id)
            .Distinct()
            .ToArray();

        if (variantIds.Length == 0)
            return;

        var lookup = await GetAvailableByVariantIdsAsync(db, branchProvider, variantIds, cancellationToken)
            .ConfigureAwait(false);

        foreach (var dto in dtos)
            ApplyVariantQuantities(dto.ProductVariants, lookup);
    }

    public static void ApplyProductAggregateStocks(
        IReadOnlyList<ProductGetSingle.Single> dtos,
        IReadOnlyDictionary<int, decimal> totalsByProductId)
    {
        foreach (var dto in dtos)
            dto.TotalAvailableQuantity = totalsByProductId.GetValueOrDefault(dto.Id, 0m);
    }

    private static void ApplyVariantQuantities(
        IReadOnlyList<ProductVariation.Response.ProductVariantDto>? variants,
        IReadOnlyDictionary<int, decimal> quantityByVariantId)
    {
        if (variants is not { Count: > 0 })
            return;

        foreach (var v in variants)
            v.AvailableQuantity = quantityByVariantId.GetValueOrDefault(v.Id, 0m);
    }
}
