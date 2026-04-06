namespace NextErp.Application.DTOs
{
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

        public int UserCount { get; set; }

        public string PermissionSummary { get; set; } = string.Empty;

        public List<string> Permissions { get; set; } = new();
    }

    public class IdentityUserEntry
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; }
    }

    public class IdentityBranchEntry
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class PatchUserDto
    {
        public Guid? BranchId { get; set; }
        public string? RoleName { get; set; }
    }
}
