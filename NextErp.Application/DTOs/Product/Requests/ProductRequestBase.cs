using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NextErp.Application.DTOs.Product;

public abstract record ProductRequestBase
{
    public string Title { get; set; } = null!;
    public string? Code { get; set; }
    public decimal Price { get; set; }
    public int? CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? Image { get; set; }
    public int? ParentId { get; set; }
    public int? UnitOfMeasureId { get; set; }
    public ProductMetadataRequest Metadata { get; set; } = new();

    public List<ProductImageSlotRequest> ImageSlots { get; set; } = new();

    public bool ClearGallery { get; set; }

    public List<ProductImageThumbnailUpdateRequest> ProductImageThumbnailUpdates { get; set; } = new();

    [BindNever]
    public List<GalleryResolvedSlot>? ResolvedGallery { get; set; }
}
