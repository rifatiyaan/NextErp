namespace NextErp.Application.DTOs
{
    /// <summary>
    /// Customer DTO hierarchy using nested partial classes
    /// </summary>
    public partial class Customer
    {
        public partial class Request
        {
            /// <summary>
            /// Base request properties for Customer
            /// </summary>
            public abstract class Base
            {
                public string Title { get; set; } = null!;
                public string? Email { get; set; }
                public string? Phone { get; set; }
                public string? Address { get; set; }
                public Metadata Metadata { get; set; } = new();
            }

            public partial class Get
            {
                /// <summary>
                /// Request to get a single customer by Id
                /// </summary>
                public class Single
                {
                    public Guid Id { get; set; }
                }

                /// <summary>
                /// Request to get multiple customers with pagination
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
                /// Request to create a single customer
                /// </summary>
                public class Single : Base
                {
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to create multiple customers
                /// </summary>
                public class Bulk
                {
                    public List<Single> Customers { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                /// <summary>
                /// Request to update a single customer (includes soft delete via IsActive)
                /// </summary>
                public new class Single : Base
                {
                    public Guid Id { get; set; }
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to update multiple customers
                /// </summary>
                public new class Bulk
                {
                    public List<Single> Customers { get; set; } = new();
                }
            }

            /// <summary>
            /// Customer metadata
            /// </summary>
            public class Metadata
            {
                public string? LoyaltyCode { get; set; }
                public string? Notes { get; set; }
                public string? NationalId { get; set; }
            }
        }

        public partial class Response
        {
            /// <summary>
            /// Base response properties for Customer
            /// </summary>
            public abstract class Base
            {
                public Guid Id { get; set; }
                public string Title { get; set; } = null!;
                public string? Email { get; set; }
                public string? Phone { get; set; }
                public string? Address { get; set; }
                public bool IsActive { get; set; }
                public DateTime CreatedAt { get; set; }
                public DateTime? UpdatedAt { get; set; }
                public Guid TenantId { get; set; }
                public Guid? BranchId { get; set; }
            }

            public partial class Get
            {
                /// <summary>
                /// Response for getting a single customer
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for getting multiple customers
                /// </summary>
                public class Bulk
                {
                    public List<Single> Customers { get; set; } = new();
                    public int TotalCount { get; set; }
                    public int Page { get; set; }
                    public int PageSize { get; set; }
                    public int TotalPages { get; set; }
                }
            }

            public partial class Create
            {
                /// <summary>
                /// Response for creating a single customer
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for creating multiple customers
                /// </summary>
                public class Bulk
                {
                    public List<Single> Customers { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                /// <summary>
                /// Response for updating a single customer
                /// </summary>
                public new class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for updating multiple customers
                /// </summary>
                public new class Bulk
                {
                    public List<Single> Customers { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }
        }
    }
}
