using Microsoft.AspNetCore.Identity;

namespace NextErp.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }
        public string? Sex { get; set; }
        public string? Address { get; set; }

        public string? Photo { get; set; }

        // Organization / Branch reference
        public int OrgUnitId { get; set; }
        public int? AreaId { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
