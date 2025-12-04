using Microsoft.AspNetCore.Identity;

namespace NextErp.Infrastructure.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;
        public DateTime ExpiryDate { get; set; }
    }

}
