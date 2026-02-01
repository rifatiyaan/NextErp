using Microsoft.AspNetCore.Http;

namespace NextErp.Application.DTOs
{
    public partial class Product
    {
        public partial class Request
        {
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

            public partial class Update : Create
            {
                public new class Single : Base
                {
                    public int Id { get; set; }
                    public bool IsActive { get; set; } = true;
                }

                public new class Bulk
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
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                    public Category.Response.Get.Single? Category { get; set; }
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

            public partial class Update : Create
            {
                public new class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                public new class Bulk
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
