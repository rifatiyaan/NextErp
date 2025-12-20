using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs
{
    /// <summary>
    /// Module DTO hierarchy using nested partial classes
    /// </summary>
    public partial class Module
    {
        public partial class Request
        {
            /// <summary>
            /// Base request properties for Module
            /// </summary>
            public abstract class Base
            {
                public string Title { get; set; } = null!;
                public string? Icon { get; set; }
                public string? Url { get; set; }
                public int? ParentId { get; set; }
                public ModuleType Type { get; set; } = ModuleType.Link;
                
                // Module-specific (used when Type = Module)
                public string? Description { get; set; }
                public string? Version { get; set; }
                public bool IsInstalled { get; set; }
                public bool IsEnabled { get; set; }
                
                // Common
                public int Order { get; set; }
                public bool IsExternal { get; set; }
                
                public Metadata Metadata { get; set; } = new();
            }

            public partial class Get
            {
                /// <summary>
                /// Request to get a single module by Id
                /// </summary>
                public class Single
                {
                    public int Id { get; set; }
                }

                /// <summary>
                /// Request to get multiple modules with filtering
                /// </summary>
                public class Bulk
                {
                    public int Page { get; set; } = 1;
                    public int PageSize { get; set; } = 10;
                    public string? SearchTerm { get; set; }
                    public bool? IsActive { get; set; }
                    public ModuleType? Type { get; set; }
                    public int? ParentId { get; set; }
                    public string? SortBy { get; set; }
                    public bool SortDescending { get; set; }
                }
            }

            public partial class Create
            {
                /// <summary>
                /// Request to create a single module
                /// </summary>
                public class Single : Base
                {
                    public bool IsActive { get; set; } = true;
                }

                /// <summary>
                /// Request to create multiple modules (supports hierarchical creation)
                /// </summary>
                public class Bulk
                {
                    public List<Hierarchical> Modules { get; set; } = new();
                }

                /// <summary>
                /// Hierarchical module DTO for bulk creation (supports nested children)
                /// </summary>
                public class Hierarchical : Base
                {
                    public bool IsActive { get; set; } = true;
                    public List<Hierarchical> Children { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                /// <summary>
                /// Request to update a single module (includes soft delete via IsActive)
                /// </summary>
                public new class Single : Base
                {
                    public int Id { get; set; }
                    public bool IsActive { get; set; } = true;
                    public DateTime? InstalledAt { get; set; }
                }

                /// <summary>
                /// Request to update multiple modules
                /// </summary>
                public new class Bulk
                {
                    public List<Single> Modules { get; set; } = new();
                }
            }

            /// <summary>
            /// Module metadata
            /// </summary>
            public class Metadata
            {
                // From MenuItem
                public string[]? Roles { get; set; }
                public string? BadgeText { get; set; }
                public string? BadgeColor { get; set; }
                public string? Description { get; set; }
                public bool OpenInNewTab { get; set; }

                // From Module
                public string? Author { get; set; }
                public string? Website { get; set; }
                public string[]? Dependencies { get; set; }
                public string? ConfigurationUrl { get; set; }
            }
        }

        public partial class Response
        {
            /// <summary>
            /// Base response properties for Module
            /// </summary>
            public abstract class Base
            {
                public int Id { get; set; }
                public string Title { get; set; } = null!;
                public string? Icon { get; set; }
                public string? Url { get; set; }
                public int? ParentId { get; set; }
                public ModuleType Type { get; set; }
                
                // Module-specific
                public string? Description { get; set; }
                public string? Version { get; set; }
                public bool IsInstalled { get; set; }
                public bool IsEnabled { get; set; }
                public DateTime? InstalledAt { get; set; }
                
                // Common
                public int Order { get; set; }
                public bool IsActive { get; set; }
                public bool IsExternal { get; set; }
                
                public DateTime CreatedAt { get; set; }
                public DateTime? UpdatedAt { get; set; }
                public Guid TenantId { get; set; }
            }

            public partial class Get
            {
                /// <summary>
                /// Response for getting a single module
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                    public List<Single> Children { get; set; } = new();
                }

                /// <summary>
                /// Response for getting multiple modules
                /// </summary>
                public class Bulk
                {
                    public List<Single> Modules { get; set; } = new();
                    public int TotalCount { get; set; }
                    public int Page { get; set; }
                    public int PageSize { get; set; }
                    public int TotalPages { get; set; }
                }
            }

            public partial class Create
            {
                /// <summary>
                /// Response for creating a single module
                /// </summary>
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for creating multiple modules (supports hierarchical response)
                /// </summary>
                public class Bulk
                {
                    public List<Hierarchical> Modules { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }

                /// <summary>
                /// Hierarchical module response for bulk creation
                /// </summary>
                public class Hierarchical : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                    public List<Hierarchical> Children { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                /// <summary>
                /// Response for updating a single module
                /// </summary>
                public new class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                /// <summary>
                /// Response for updating multiple modules
                /// </summary>
                public new class Bulk
                {
                    public List<Single> Modules { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }
        }
    }
}
