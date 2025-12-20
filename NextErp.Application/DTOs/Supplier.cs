namespace NextErp.Application.DTOs
{
    /// <summary>
    /// Supplier DTO hierarchy using nested partial classes
    /// </summary>
    public partial class Supplier
    {
        public partial class Request
        {
            /// <summary>
            /// Base request properties for Supplier
            /// </summary>
            public abstract class Base
            {
                public string Title { get; set; } = null!;
                public string? ContactPerson { get; set; }
                public string? Phone { get; set; }
                public string? Email { get; set; }
                public string? Address { get; set; }
                public Metadata Metadata { get; set; } = new();
            }

            public partial class Get
            {
                /// <summary>
                /// Request to get a single supplier by Id
                /// </summary>
                public class Single
                {
                    public int Id { get; set; }
                }

                /// <summary>
                /// Request to get multiple suppliers with pagination
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
                /// Request to create a single supplier
                /// </summary>
                public class Single : Base
                {
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to create multiple suppliers
                /// </summary>
                public class Bulk
                {
                    public List<Single> Suppliers { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                /// <summary>
                /// Request to update a single supplier (includes soft delete via IsActive)
                /// </summary>
                public new class Single : Base
                {
                    public int Id { get; set; }
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to update multiple suppliers
                /// </summary>
                public new class Bulk
                {
                    public List<Single> Suppliers { get; set; } = new();
                }
            }

            /// <summary>
            /// Supplier metadata
            /// </summary>
            public class Metadata
            {
                public string? VatNumber { get; set; }
                public string? TaxId { get; set; }
                public string? Notes { get; set; }
            }
        }

        public partial class Response
        {
            /// <summary>
            /// Base response properties for Supplier
            /// </summary>
            public abstract class Base
            {
                public int Id { get; set; }
                public string Title { get; set; } = null!;
                public string? ContactPerson { get; set; }
                public string? Phone { get; set; }
                public string? Email { get; set; }
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
                /// Response for getting a single supplier
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for getting multiple suppliers
                /// </summary>
                public class Bulk
                {
                    public List<Single> Suppliers { get; set; } = new();
                    public int TotalCount { get; set; }
                    public int Page { get; set; }
                    public int PageSize { get; set; }
                    public int TotalPages { get; set; }
                }
            }

            public partial class Create
            {
                /// <summary>
                /// Response for creating a single supplier
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for creating multiple suppliers
                /// </summary>
                public class Bulk
                {
                    public List<Single> Suppliers { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                /// <summary>
                /// Response for updating a single supplier
                /// </summary>
                public new class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for updating multiple suppliers
                /// </summary>
                public new class Bulk
                {
                    public List<Single> Suppliers { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }
        }
    }
}
