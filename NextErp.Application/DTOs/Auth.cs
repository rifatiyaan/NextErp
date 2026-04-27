namespace NextErp.Application.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Username { get; set; }
        public Guid BranchId { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public bool IsSuperAdmin { get; set; }
        public IReadOnlyList<string> PermissionKeys { get; set; } = Array.Empty<string>();
    }

    public class CurrentUserDto
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsGlobal { get; set; }
        public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
        public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
    }
}
