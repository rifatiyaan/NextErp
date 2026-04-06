namespace NextErp.Domain.Entities
{
    public class RolePermission
    {
        public Guid Id { get; set; }

        public Guid RoleId { get; set; }

        public string PermissionKey { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
