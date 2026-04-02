namespace NextErp.Domain.Entities
{
    /// <summary>
    /// Persists which permission keys are granted to a specific role.
    /// Replaces the static dictionary in GetIdentityDashboardHandler.
    /// </summary>
    public class RolePermission
    {
        public Guid Id { get; set; }

        /// <summary>Maps to IdentityRole&lt;Guid&gt;.Id.</summary>
        public Guid RoleId { get; set; }

        /// <summary>Dot-notation capability key, e.g. "create_sale".</summary>
        public string PermissionKey { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
