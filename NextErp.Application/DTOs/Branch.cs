namespace NextErp.Application.DTOs
{
    public partial class Branch
    {
        public partial class Request
        {
            public abstract class Base
            {
                public string Name { get; set; } = null!;
                public string? Address { get; set; }
                public Metadata Metadata { get; set; } = new();
            }

            public partial class Get
            {
                public class Single
                {
                    public Guid Id { get; set; }
                }

                public class Bulk
                {
                    public int Page { get; set; } = 1;
                    public int PageSize { get; set; } = 10;
                    public string? SearchTerm { get; set; }
                    public bool? IsActive { get; set; }
                    public Guid? TenantId { get; set; }
                    public string? SortBy { get; set; }
                    public bool SortDescending { get; set; }
                }
            }

            public partial class Create
            {
                public class Single : Base
                {
                    public bool IsActive { get; set; } = true;
                }

                public class Bulk
                {
                    public List<Single> Branches { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                public new class Single : Base
                {
                    public Guid Id { get; set; }
                    public bool IsActive { get; set; } = true;
                }

                public new class Bulk
                {
                    public List<Single> Branches { get; set; } = new();
                }
            }

            public class Metadata
            {
                public string? Phone { get; set; }
                public string? ManagerName { get; set; }
                public string? BranchCode { get; set; }
                public string? Email { get; set; }
            }
        }

        public partial class Response
        {
            public abstract class Base
            {
                public Guid Id { get; set; }
                public Guid TenantId { get; set; }
                public string Name { get; set; } = null!;
                public string? Address { get; set; }
                public bool IsActive { get; set; }
                public DateTime CreatedAt { get; set; }
                public DateTime? UpdatedAt { get; set; }
            }

            public partial class Get
            {
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                    public Tenant.Response.Get.Single? Tenant { get; set; }
                }

                public class Bulk
                {
                    public List<Single> Branches { get; set; } = new();
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
                    public List<Single> Branches { get; set; } = new();
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
                    public List<Single> Branches { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }
        }
    }
}
