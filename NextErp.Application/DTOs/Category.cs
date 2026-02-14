namespace NextErp.Application.DTOs
{
    public partial class Category
    {
        public partial class Request
        {
            public abstract class Base
            {
                public string Title { get; set; } = null!;
                public string? Description { get; set; }
                public int? ParentId { get; set; }
                public Metadata Metadata { get; set; } = new();
                public List<Asset> Assets { get; set; } = new();
            }

            public class Asset
            {
                public string Filename { get; set; } = null!;
                public string Url { get; set; } = null!;
                public string Type { get; set; } = "image";
                public long? Size { get; set; }
                public DateTime UploadedAt { get; set; }
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
                    public bool? IsActive { get; set; }
                    public int? ParentId { get; set; }
                    public string? SortBy { get; set; }
                    public bool SortDescending { get; set; }
                }
            }

            public partial class Create
            {
                public class Single : Base
                {
                    public Microsoft.AspNetCore.Http.IFormFile[]? Images { get; set; }
                }

                public class Bulk
                {
                    public List<Single> Categories { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                public new class Single : Base
                {
                    public int Id { get; set; }
                    public Microsoft.AspNetCore.Http.IFormFile[]? Images { get; set; }
                }

                public new class Bulk
                {
                    public List<Single> Categories { get; set; } = new();
                }
            }

            public class Metadata
            {
                public string? ProductCount { get; set; }
                public string? Department { get; set; }
            }
        }

        public partial class Response
        {
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
                public List<Request.Asset> Assets { get; set; } = new();
            }

            public partial class Get
            {
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                    public List<Product.Response.Get.Single>? Products { get; set; }
                }

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
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

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
                public new class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

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
