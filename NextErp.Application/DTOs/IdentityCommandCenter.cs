namespace NextErp.Application.DTOs
{
    /// <summary>
    /// Aggregated payload for the Identity Command Center BFF endpoint.
    /// One call replaces separate role, user, and branch requests.
    /// </summary>
    public class IdentityCommandCenterDto
    {
        public List<IdentityRoleEntry> Roles { get; set; } = new();
        public List<IdentityUserEntry> Users { get; set; } = new();
        public List<IdentityBranchEntry> Branches { get; set; } = new();
    }

    public class IdentityRoleEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        /// <summary>How many users currently hold this role.</summary>
        public int UserCount { get; set; }

        /// <summary>Human-readable access level label (e.g. "Full Access", "Branch Limited").</summary>
        public string PermissionSummary { get; set; } = string.Empty;

        /// <summary>Flat list of capability keys shown in the permission matrix.</summary>
        public List<string> Permissions { get; set; } = new();
    }

    public class IdentityUserEntry
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        /// <summary>Photo URL stored on ApplicationUser. Null when no photo is set.</summary>
        public string? AvatarUrl { get; set; }

        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; }
    }

    /// <summary>Lean branch record used to populate assignment dropdowns.</summary>
    public class IdentityBranchEntry
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>Payload for PATCH /api/identity/users/{id}.</summary>
    public class PatchUserDto
    {
        public Guid? BranchId { get; set; }
        public string? RoleName { get; set; }
    }
}
