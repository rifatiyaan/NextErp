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
}
