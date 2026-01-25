namespace NextErp.API.Areas.Admin.DTO
{
    public class RegisterDto
    {
        // Optional: UI may only send email/password. If provided, we'll use it for UserName.
        public string? Username { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
