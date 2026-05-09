using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.HasIndex(n => new { n.TenantId, n.BranchId, n.UserId, n.ReadAt });
        builder.HasIndex(n => n.CreatedAt);

        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Type).IsRequired().HasMaxLength(64);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);
        builder.Property(n => n.RelatedEntityType).HasMaxLength(64);
        builder.Property(n => n.RelatedEntityId).HasMaxLength(64);
    }
}
