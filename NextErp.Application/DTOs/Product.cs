using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NextErp.Application.DTOs
{
    public partial class Product
    {
        public partial class Request
        {
            /// <summary>Multipart slot: existing URL and/or new file; <see cref="IsThumbnail"/> marks the primary image for this request.</summary>
            public class ImageSlot
            {
                public string? Url { get; set; }
                public IFormFile? File { get; set; }
                public bool IsThumbnail { get; set; }
            }

            /// <summary>Gallery row after the API resolves uploads (used by commands).</summary>
            public record GalleryResolvedSlot(string Url, bool IsThumbnail);

            /// <summary>Update existing product image rows: only IsThumbnail changes.</summary>
            public class ProductImageThumbnailUpdate
            {
                public int Id { get; set; }
                public bool IsThumbnail { get; set; }
            }

            public abstract class Base
            {
                public string Title { get; set; } = null!;
                public string Code { get; set; } = null!;
                public decimal Price { get; set; }
                public int Stock { get; set; }
                public int? CategoryId { get; set; }
                public string? ImageUrl { get; set; }
                public IFormFile? Image { get; set; }
                public int? ParentId { get; set; }
                public Metadata Metadata { get; set; } = new();

                /// <summary>Ordered gallery: model-bound as ImageSlots[i].Url, .File, .IsThumbnail.</summary>
                public List<ImageSlot> ImageSlots { get; set; } = new();

                /// <summary>When true, clears all product images (no slots required).</summary>
                public bool ClearGallery { get; set; }

                /// <summary>Per-image thumbnail flags for existing rows (update). Bound as ProductImageThumbnailUpdates[i].Id / .IsThumbnail.</summary>
                public List<ProductImageThumbnailUpdate> ProductImageThumbnailUpdates { get; set; } = new();

                /// <summary>Set by the API after resolving uploads.</summary>
                [BindNever]
                public List<GalleryResolvedSlot>? ResolvedGallery { get; set; }
            }

            public partial class Get
            {
                public class Single
                {
                    public int Id { get; set; }
                }

                public class Bulk
                {
                    public int Page { get; set; } = 1;
                    public int PageSize { get; set; } = 10;
                    public string? SearchTerm { get; set; }
                    public int? CategoryId { get; set; }
                    public bool? IsActive { get; set; }
                    public string? SortBy { get; set; }
                    public bool SortDescending { get; set; }
                }
            }

            public partial class Create
            {
                public class Single : Base
                {
                    public bool IsActive { get; set; } = true;
                    
                    // Variation system support (optional - if null/empty, product has no variations)
                    public bool HasVariations { get; set; } = false;
                    public List<ProductVariation.Request.VariationOptionDto>? VariationOptions { get; set; } // null = simple product
                    public List<ProductVariation.Request.ProductVariantDto>? ProductVariants { get; set; } // null = simple product
                }

                public class Bulk
                {
                    public List<Single> Products { get; set; } = new();
                }
            }

            public partial class Update
            {
                public class Single : Base
                {
                    public int Id { get; set; }
                    public bool IsActive { get; set; } = true;
                    
                    // Variation system support (optional - if null/empty, product has no variations)
                    public bool HasVariations { get; set; } = false;
                    public List<ProductVariation.Request.VariationOptionDto>? VariationOptions { get; set; } // null = simple product
                    public List<ProductVariation.Request.ProductVariantDto>? ProductVariants { get; set; } // null = simple product
                }

                public class Bulk
                {
                    public List<Single> Products { get; set; } = new();
                }
            }

            public class Metadata
            {
                public string? Description { get; set; }
                public string? Color { get; set; }
                public string? Warranty { get; set; }
                public int? CategoryId { get; set; }
            }
        }

        public partial class Response
        {
            public abstract class Base
            {
                public int Id { get; set; }
                public string Title { get; set; } = null!;
                public string Code { get; set; } = null!;
                public decimal Price { get; set; }
                public int Stock { get; set; }
                public int? CategoryId { get; set; }
                public string? ImageUrl { get; set; }
                public int? ParentId { get; set; }
                public bool IsActive { get; set; }
                public DateTime CreatedAt { get; set; }
                public DateTime? UpdatedAt { get; set; }
                public Guid TenantId { get; set; }
                public Guid? BranchId { get; set; }
            }

            public partial class Get
            {
                public class ProductImageItem
                {
                    public int Id { get; set; }
                    public string Url { get; set; } = null!;
                    public int DisplayOrder { get; set; }
                    public bool IsThumbnail { get; set; }
                }

                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                    public Category.Response.Get.Single? Category { get; set; }
                    public bool HasVariations { get; set; }
                    public List<ProductVariation.Response.VariationOptionDto>? VariationOptions { get; set; }
                    public List<ProductVariation.Response.ProductVariantDto>? ProductVariants { get; set; }
                    public List<ProductImageItem>? Images { get; set; }

                    /// <summary>Set when list API is called with includeStock=true (sum of variant ledger qty).</summary>
                    public decimal? TotalAvailableQuantity { get; set; }

                    /// <summary>True if any variant ledger is at or below low threshold.</summary>
                    public bool? HasLowStock { get; set; }
                }

                public class Bulk
                {
                    public List<Single> Products { get; set; } = new();
                    public int TotalCount { get; set; }
                    public int Page { get; set; }
                    public int PageSize { get; set; }
                    public int TotalPages { get; set; }
                }
            }

            public partial class Create
            {
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                public class Bulk
                {
                    public List<Single> Products { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }

            public partial class Update
            {
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                public class Bulk
                {
                    public List<Single> Products { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }
        }
    }
}
