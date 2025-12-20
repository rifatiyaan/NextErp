namespace NextErp.Application.DTOs
{
    /// <summary>
    /// Tenant DTO hierarchy using nested partial classes
    /// </summary>
    public partial class Tenant
    {
        public partial class Request
        {
            /// <summary>
            /// Base request properties for Tenant
            /// </summary>
            public abstract class Base
            {
                public string Name { get; set; } = null!;
                public string? DatabaseConnectionString { get; set; }
                public Metadata Metadata { get; set; } = new();
            }

            public partial class Get
            {
                /// <summary>
                /// Request to get a single tenant by Id
                /// </summary>
                public class Single
                {
                    public Guid Id { get; set; }
                }

                /// <summary>
                /// Request to get multiple tenants with pagination
                /// </summary>
                public class Bulk
                {
                    public int Page { get; set; } = 1;
                    public int PageSize { get; set; } = 10;
                    public string? SearchTerm { get; set; }
                    public bool? IsActive { get; set; }
                    public string? SortBy { get; set; }
                    public bool SortDescending { get; set; }
                }
            }

            public partial class Create
            {
                /// <summary>
                /// Request to create a single tenant
                /// </summary>
                public class Single : Base
                {
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to create multiple tenants
                /// </summary>
                public class Bulk
                {
                    public List<Single> Tenants { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                /// <summary>
                /// Request to update a single tenant (includes soft delete via IsActive)
                /// </summary>
                public new class Single : Base
                {
                    public Guid Id { get; set; }
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to update multiple tenants
                /// </summary>
                public new class Bulk
                {
                    public List<Single> Tenants { get; set; } = new();
                }
            }

            /// <summary>
            /// Tenant metadata
            /// </summary>
            public class Metadata
            {
                public string? AdminEmail { get; set; }
                public string? SubscriptionPlan { get; set; }
                public DateTime? SubscriptionExpiry { get; set; }
            }
        }

        public partial class Response
        {
            /// <summary>
            /// Base response properties for Tenant
            /// </summary>
            public abstract class Base
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = null!;
                public string? DatabaseConnectionString { get; set; }
                public bool IsActive { get; set; }
                public DateTime CreatedAt { get; set; }
                public DateTime? UpdatedAt { get; set; }
            }

            public partial class Get
            {
                /// <summary>
                /// Response for getting a single tenant
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                    public List<Branch.Response.Get.Single>? Branches { get; set; }
                }

                /// <summary>
                /// Response for getting multiple tenants
                /// </summary>
                public class Bulk
                {
                    public List<Single> Tenants { get; set; } = new();
                    public int TotalCount { get; set; }
                    public int Page { get; set; }
                    public int PageSize { get; set; }
                    public int TotalPages { get; set; }
                }
            }

            public partial class Create
            {
                /// <summary>
                /// Response for creating a single tenant
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for creating multiple tenants
                /// </summary>
                public class Bulk
                {
                    public List<Single> Tenants { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                /// <summary>
                /// Response for updating a single tenant
                /// </summary>
                public new class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for updating multiple tenants
                /// </summary>
                public new class Bulk
                {
                    public List<Single> Tenants { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }
        }
    }
}
