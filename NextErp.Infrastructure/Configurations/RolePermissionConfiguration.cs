using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.HasKey(rp => rp.Id);
            builder.Property(rp => rp.Id).ValueGeneratedNever();

            builder.Property(rp => rp.PermissionKey).IsRequired().HasMaxLength(100);

            // One role can have many permission keys — enforce uniqueness per pair
            builder.HasIndex(rp => new { rp.RoleId, rp.PermissionKey }).IsUnique();
            builder.HasIndex(rp => rp.RoleId);
        }
    }
}
