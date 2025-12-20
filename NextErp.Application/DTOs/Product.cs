namespace NextErp.Application.DTOs
{
    /// <summary>
    /// Product DTO hierarchy using nested partial classes
    /// </summary>
    public partial class Product
    {
        public partial class Request
        {
            /// <summary>
            /// Base request properties for Product
            /// </summary>
            public abstract class Base
            {
                public string Title { get; set; } = null!;
                public string Code { get; set; } = null!;
                public decimal Price { get; set; }
                public int Stock { get; set; }
                public int? CategoryId { get; set; }
                public string? ImageUrl { get; set; }
                public int? ParentId { get; set; }
                public Metadata Metadata { get; set; } = new();
            }

            public partial class Get
            {
                /// <summary>
                /// Request to get a single product by Id
                /// </summary>
                public class Single
                {
                    public int Id { get; set; }
                }

                /// <summary>
                /// Request to get multiple products with pagination
                /// </summary>
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
                /// <summary>
                /// Request to create a single product
                /// </summary>
                public class Single : Base
                {
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to create multiple products
                /// </summary>
                public class Bulk
                {
                    public List<Single> Products { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                /// <summary>
                /// Request to update a single product (includes soft delete via IsActive)
                /// </summary>
                public new class Single : Base
                {
                    public int Id { get; set; }
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to update multiple products
                /// </summary>
                public new class Bulk
                {
                    public List<Single> Products { get; set; } = new();
                }
            }

            /// <summary>
            /// Product metadata
            /// </summary>
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
            /// <summary>
            /// Base response properties for Product
            /// </summary>
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
                /// <summary>
                /// Response for getting a single product
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                    public Category.Response.Get.Single? Category { get; set; }
                }

                /// <summary>
                /// Response for getting multiple products
                /// </summary>
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
                /// <summary>
                /// Response for creating a single product
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for creating multiple products
                /// </summary>
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
                /// <summary>
                /// Response for updating a single product
                /// </summary>
                public new class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for updating multiple products
                /// </summary>
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
