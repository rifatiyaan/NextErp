using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using DomainEntities = NextErp.Domain.Entities;
using DomainProduct = NextErp.Domain.Entities.Product;

namespace NextErp.Application.Products;

public static class ProductGallerySync
{
    public static async Task ApplyFullGalleryAsync(
            DomainProduct product,
            IReadOnlyList<Product.Request.GalleryResolvedSlot>? gallery,
            IApplicationDbContext db,
            CancellationToken cancellationToken)
    {
        if (gallery == null)
            return;

        var items = gallery
            .Where(s => !string.IsNullOrWhiteSpace(s.Url))
            .Select(s => new Product.Request.GalleryResolvedSlot(s.Url.Trim(), s.IsThumbnail))
            .ToList();

        var existing = await db.ProductImages
            .Where(pi => pi.ProductId == product.Id)
            .ToListAsync(cancellationToken);
        db.ProductImages.RemoveRange(existing);

        if (items.Count == 0)
        {
            product.ImageUrl = null;
            return;
        }

        var thumbIndex = items.FindIndex(i => i.IsThumbnail);
        if (thumbIndex < 0)
            thumbIndex = 0;

        for (var i = 0; i < items.Count; i++)
        {
            await db.ProductImages.AddAsync(
                new DomainEntities.ProductImage
                {
                    ProductId = product.Id,
                    Url = items[i].Url,
                    DisplayOrder = i,
                    IsThumbnail = i == thumbIndex,
                    Title = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                },
                cancellationToken);
        }

        product.ImageUrl = items[thumbIndex].Url;
    }

    public static async Task ApplyThumbnailUpdatesAsync(
            int productId,
            IReadOnlyList<Product.Request.ProductImageThumbnailUpdate> updates,
            DomainProduct product,
            IApplicationDbContext db,
            CancellationToken cancellationToken)
    {
        if (updates == null || updates.Count == 0)
            return;

        var rows = await db.ProductImages
            .Where(pi => pi.ProductId == productId)
            .ToListAsync(cancellationToken);

        foreach (var u in updates)
        {
            var row = rows.FirstOrDefault(r => r.Id == u.Id);
            if (row != null)
                row.IsThumbnail = u.IsThumbnail;
        }

        var thumbRows = rows.Where(r => r.IsThumbnail).ToList();
        if (thumbRows.Count > 1)
        {
            foreach (var t in thumbRows.Skip(1))
                t.IsThumbnail = false;
        }

        if (!rows.Any(r => r.IsThumbnail) && rows.Count > 0)
            rows[0].IsThumbnail = true;

        var primary = rows.FirstOrDefault(r => r.IsThumbnail) ?? rows.FirstOrDefault();
        product.ImageUrl = primary?.Url;
    }
}
