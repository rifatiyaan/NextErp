using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs
{
    public partial class Module
    {
        public partial class Request
        {
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
                    public ModuleType? Type { get; set; }
                    public int? ParentId { get; set; }
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
                    public List<Hierarchical> Modules { get; set; } = new();
                }

                public class Hierarchical : Base
                {
                    public bool IsActive { get; set; } = true;
                    public List<Hierarchical> Children { get; set; } = new();
                }
            }

            public partial class Update : Create
            {
                public new class Single : Base
                {
                    public int Id { get; set; }
                    public bool IsActive { get; set; } = true;
                    public DateTime? InstalledAt { get; set; }
                }

                public new class Bulk
                {
                    public List<Single> Modules { get; set; } = new();
                }
            }

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
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                    public List<Single> Children { get; set; } = new();
                }

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
                public class Single : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                }

                public class Bulk
                {
                    public List<Hierarchical> Modules { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }

                public class Hierarchical : Base
                {
                    public Request.Metadata Metadata { get; set; } = new();
                    public List<Hierarchical> Children { get; set; } = new();
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
                    public List<Single> Modules { get; set; } = new();
                    public int SuccessCount { get; set; }
                    public int FailureCount { get; set; }
                    public List<string> Errors { get; set; } = new();
                }
            }
        }
    }
}
