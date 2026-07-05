using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Common.Settings;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Application.Settings;

namespace NextErp.Application.Handlers.QueryHandlers.Ecommerce;

internal static class StoreQueryShared
{
    // Anonymous requests carry no branch claim, so the [BranchScoped] global
    // filter cannot apply — every store query bypasses it and pins the branch
    // to the configured selling branch explicitly.
    public static async Task<Guid> SellingBranchAsync(ISettingsProvider settings)
    {
        var s = await settings.GetAsync<EcommerceSettings>();
        return Guid.TryParse(s.SellingBranchId, out var id) ? id : Guid.Empty;
    }

    public static decimal? LowStock(decimal available) =>
        available > 0 && available <= 5 ? available : null;

    // Storefront-visible products: active + published, in a published category,
    // pinned to the selling branch. Optionally narrowed to one category. Shared
    // by the paged listing and the price-range facet so both see the same set.
    public static IQueryable<Domain.Entities.Product> PublishedProducts(
        IApplicationDbContext dbContext, Guid branchId, int? categoryId)
    {
        var query = dbContext.Products
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.IsActive && p.IsPublishedOnline && p.BranchId == branchId
                        && p.Category.IsActive && p.Category.IsPublishedOnline);

        if (categoryId is int id)
            query = query.Where(p => p.CategoryId == id);

        return query;
    }
}

public class GetStoreConfigHandler(ISettingsProvider settings)
    : IRequestHandler<GetStoreConfigQuery, StoreConfigResponse>
{
    public async Task<StoreConfigResponse> Handle(GetStoreConfigQuery request, CancellationToken cancellationToken = default)
    {
        var s = await settings.GetAsync<EcommerceSettings>();
        return new StoreConfigResponse(
            s.StorefrontEnabled, s.StoreName, s.Tagline, s.HeroHeadline,
            s.HeroImageUrl, s.MarqueeText, s.CodNote, s.DeliveryFee);
    }
}

public class GetStoreCategoriesHandler(IApplicationDbContext dbContext, ISettingsProvider settings)
    : IRequestHandler<GetStoreCategoriesQuery, List<StoreCategoryResponse>>
{
    public async Task<List<StoreCategoryResponse>> Handle(GetStoreCategoriesQuery request, CancellationToken cancellationToken = default)
    {
        var branchId = await StoreQueryShared.SellingBranchAsync(settings);

        var categories = await dbContext.Categories
            .AsNoTracking()
            .Where(c => c.IsActive && c.IsPublishedOnline)
            .OrderBy(c => c.Title)
            .Select(c => new
            {
                c.Id, c.Title, c.ParentId,
                ImageUrl = c.Assets.Where(a => a.Type == "image").Select(a => a.Url).FirstOrDefault(),
                ProductCount = dbContext.Products
                    .IgnoreQueryFilters()
                    .Count(p => p.CategoryId == c.Id && p.IsActive && p.IsPublishedOnline && p.BranchId == branchId),
            })
            .ToListAsync(cancellationToken);

        return categories
            .Where(c => c.ProductCount > 0)
            .Select(c => new StoreCategoryResponse(c.Id, c.Title, c.ParentId, c.ProductCount, c.ImageUrl))
            .ToList();
    }
}

public class GetStorePagedProductsHandler(IApplicationDbContext dbContext, ISettingsProvider settings)
    : IRequestHandler<GetStorePagedProductsQuery, StorePagedProductsResponse>
{
    public async Task<StorePagedProductsResponse> Handle(GetStorePagedProductsQuery request, CancellationToken cancellationToken = default)
    {
        var branchId = await StoreQueryShared.SellingBranchAsync(settings);
        var pageIndex = Math.Max(1, request.PageIndex);
        var pageSize = Math.Clamp(request.PageSize, 1, 60);

        var query = StoreQueryShared.PublishedProducts(dbContext, branchId, request.CategoryId);

        if (!string.IsNullOrWhiteSpace(request.SearchText))
            query = query.Where(p => p.Title.Contains(request.SearchText));
        if (request.MinPrice is decimal min)
            query = query.Where(p => p.Price >= min);
        if (request.MaxPrice is decimal max)
            query = query.Where(p => p.Price <= max);

        var total = await query.CountAsync(cancellationToken);

        var pageProducts = await query
            .OrderBy(p => p.Title)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id, p.Title, p.Price, p.HasVariations,
                Images = p.ProductImages.OrderBy(i => i.DisplayOrder).Select(i => i.Url).Take(2).ToList(),
                FallbackImage = p.ImageUrl,
                VariantIds = p.ProductVariants.Select(v => v.Id).ToList(),
            })
            .ToListAsync(cancellationToken);

        // SQLite cannot translate a correlated Sum() subquery nested inside this
        // projection, so availability is aggregated separately: one extra query
        // for the whole page (not per row), matching the detail handler.
        var pageVariantIds = pageProducts.SelectMany(p => p.VariantIds).ToList();
        var availableByVariant = await dbContext.Stocks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && pageVariantIds.Contains(s.ProductVariantId))
            .GroupBy(s => s.ProductVariantId)
            .Select(g => new { VariantId = g.Key, Available = g.Sum(s => s.AvailableQuantity) })
            .ToDictionaryAsync(x => x.VariantId, x => x.Available, cancellationToken);

        var data = pageProducts.Select(r =>
        {
            var available = r.VariantIds.Sum(id => availableByVariant.GetValueOrDefault(id, 0m));
            return new StoreProductRow(
                r.Id, r.Title, r.Price,
                r.Images.ElementAtOrDefault(0) ?? r.FallbackImage,
                r.Images.ElementAtOrDefault(1),
                available > 0,
                StoreQueryShared.LowStock(available),
                r.HasVariations);
        }).ToList();

        return new StorePagedProductsResponse(total, data);
    }
}

public class GetStoreProductByIdHandler(IApplicationDbContext dbContext, ISettingsProvider settings)
    : IRequestHandler<GetStoreProductByIdQuery, StoreProductDetailResponse?>
{
    public async Task<StoreProductDetailResponse?> Handle(GetStoreProductByIdQuery request, CancellationToken cancellationToken = default)
    {
        var branchId = await StoreQueryShared.SellingBranchAsync(settings);

        var product = await dbContext.Products
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.Id == request.Id
                        && p.IsActive && p.IsPublishedOnline && p.BranchId == branchId
                        && p.Category.IsActive && p.Category.IsPublishedOnline)
            .Select(p => new
            {
                p.Id, p.Title, p.Price,
                Description = p.Metadata.Description,
                CategoryTitle = p.Category.Title,
                p.CategoryId,
                Images = p.ProductImages.OrderBy(i => i.DisplayOrder).Select(i => i.Url).ToList(),
                FallbackImage = p.ImageUrl,
                Variants = p.ProductVariants
                    .Where(v => v.IsActive)
                    .Select(v => new { v.Id, v.Sku, v.Title, v.Price })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            return null;

        var variantIds = product.Variants.Select(v => v.Id).ToList();
        var stockByVariant = await dbContext.Stocks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && variantIds.Contains(s.ProductVariantId))
            .GroupBy(s => s.ProductVariantId)
            .Select(g => new { VariantId = g.Key, Available = g.Sum(s => s.AvailableQuantity) })
            .ToDictionaryAsync(x => x.VariantId, x => x.Available, cancellationToken);

        var images = product.Images.Count > 0
            ? product.Images
            : (product.FallbackImage is null ? new List<string>() : new List<string> { product.FallbackImage });

        var variants = product.Variants.Select(v =>
        {
            var available = stockByVariant.GetValueOrDefault(v.Id, 0m);
            return new StoreVariantRow(v.Id, v.Sku, v.Title, v.Price, available > 0, StoreQueryShared.LowStock(available));
        }).ToList();

        return new StoreProductDetailResponse(
            product.Id, product.Title, product.Price, product.Description,
            product.CategoryTitle, product.CategoryId, images, variants);
    }
}

public class GetStorePriceRangeHandler(IApplicationDbContext dbContext, ISettingsProvider settings)
    : IRequestHandler<GetStorePriceRangeQuery, StorePriceRangeResponse>
{
    public async Task<StorePriceRangeResponse> Handle(GetStorePriceRangeQuery request, CancellationToken cancellationToken = default)
    {
        var branchId = await StoreQueryShared.SellingBranchAsync(settings);
        var query = StoreQueryShared.PublishedProducts(dbContext, branchId, request.CategoryId);

        // No visible products -> a neutral 0..0 range the slider can render as empty.
        if (!await query.AnyAsync(cancellationToken))
            return new StorePriceRangeResponse(0m, 0m);

        var min = await query.MinAsync(p => p.Price, cancellationToken);
        var max = await query.MaxAsync(p => p.Price, cancellationToken);
        return new StorePriceRangeResponse(min, max);
    }
}
