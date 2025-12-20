namespace NextErp.Application.DTOs
{
    /// <summary>
    /// Category DTO hierarchy using nested partial classes
    /// </summary>
    public partial class Category
    {
        public partial class Request
        {
            /// <summary>
            /// Base request properties for Category
            /// </summary>
            public abstract class Base
            {
                public string Title { get; set; } = null!;
                public string? Description { get; set; }
                public int? ParentId { get; set; }
                public Metadata Metadata { get; set; } = new();
            }

            public partial class Get
            {
                /// <summary>
                /// Request to get a single category by Id
                /// </summary>
                public class Single
                {
                    public int Id { get; set; }
                }

                /// <summary>
                /// Request to get multiple categories with pagination
                /// </summary>
                public class Bulk
                {
                    public int Page { get; set; } = 1;
                    public int PageSize { get; set; } = 10;
                    public string? SearchTerm { get; set; }
                    public bool? IsActive { get; set; }
                    public int? ParentId { get; set; }
                    public string? SortBy { get; set; }
                    public bool SortDescending { get; set; }
                }
            }

            public partial class Create
            {
                /// <summary>
                /// Request to create a single category
                /// </summary>
                public class Single : Base
                {
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to create multiple categories
                /// </summary>
                public class Bulk
                {
                    public List<Single> Categories { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                /// <summary>
                /// Request to update a single category (includes soft delete via IsActive)
                /// </summary>
                public new class Single : Base
                {
                    public int Id { get; set; }
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to update multiple categories
                /// </summary>
                public new class Bulk
                {
                    public List<Single> Categories { get; set; } = new();
                }
            }

            /// <summary>
            /// Category metadata
            /// </summary>
            public class Metadata
            {
                public string? ProductCount { get; set; }
                public string? Department { get; set; }
            }
        }

        public partial class Response
        {
            /// <summary>
            /// Base response properties for Category
            /// </summary>
            public abstract class Base
            {
                public int Id { get; set; }
                public string Title { get; set; } = null!;
                public string? Description { get; set; }
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
                /// Response for getting a single category
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                    public List<Product.Response.Get.Single>? Products { get; set; }
                }

                /// <summary>
                /// Response for getting multiple categories
                /// </summary>
                public class Bulk
                {
                    public List<Single> Categories { get; set; } = new();
                    public int TotalCount { get; set; }
                    public int Page { get; set; }
                    public int PageSize { get; set; }
                    public int TotalPages { get; set; }
                }
            }

            public partial class Create
            {
                /// <summary>
                /// Response for creating a single category
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for creating multiple categories
                /// </summary>
                public class Bulk
                {
                    public List<Single> Categories { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                /// <summary>
                /// Response for updating a single category
                /// </summary>
                public new class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for updating multiple categories
                /// </summary>
                public new class Bulk
                {
                    public List<Single> Categories { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }
        }
    }
}
