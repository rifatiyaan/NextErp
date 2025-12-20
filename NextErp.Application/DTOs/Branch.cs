namespace NextErp.Application.DTOs
{
    /// <summary>
    /// Branch DTO hierarchy using nested partial classes
    /// </summary>
    public partial class Branch
    {
        public partial class Request
        {
            /// <summary>
            /// Base request properties for Branch
            /// </summary>
            public abstract class Base
            {
                public string Name { get; set; } = null!;
                public string? Address { get; set; }
                public Metadata Metadata { get; set; } = new();
            }

            public partial class Get
            {
                /// <summary>
                /// Request to get a single branch by Id
                /// </summary>
                public class Single
                {
                    public Guid Id { get; set; }
                }

                /// <summary>
                /// Request to get multiple branches with pagination
                /// </summary>
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
                /// <summary>
                /// Request to create a single branch
                /// </summary>
                public class Single : Base
                {
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to create multiple branches
                /// </summary>
                public class Bulk
                {
                    public List<Single> Branches { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                /// <summary>
                /// Request to update a single branch (includes soft delete via IsActive)
                /// </summary>
                public new class Single : Base
                {
                    public Guid Id { get; set; }
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to update multiple branches
                /// </summary>
                public new class Bulk
                {
                    public List<Single> Branches { get; set; } = new();
                }
            }

            /// <summary>
            /// Branch metadata
            /// </summary>
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
            /// <summary>
            /// Base response properties for Branch
            /// </summary>
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
                /// <summary>
                /// Response for getting a single branch
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                    public Tenant.Response.Get.Single? Tenant { get; set; }
                }

                /// <summary>
                /// Response for getting multiple branches
                /// </summary>
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
                /// <summary>
                /// Response for creating a single branch
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for creating multiple branches
                /// </summary>
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
                /// <summary>
                /// Response for updating a single branch
                /// </summary>
                public new class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for updating multiple branches
                /// </summary>
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
