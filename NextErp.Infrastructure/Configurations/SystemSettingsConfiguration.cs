using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class SystemSettingsConfiguration : IEntityTypeConfiguration<SystemSettings>
{
    public void Configure(EntityTypeBuilder<SystemSettings> builder)
    {
        builder.ToTable("SystemSettings");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TenantId).IsRequired();
        builder.HasIndex(s => s.TenantId).IsUnique();

        builder.Property(s => s.PresetAccentTheme).HasMaxLength(64);
        builder.Property(s => s.CustomPrimary).HasMaxLength(32);
        builder.Property(s => s.CustomSecondary).HasMaxLength(32);
        builder.Property(s => s.CustomSidebarBackground).HasMaxLength(32);
        builder.Property(s => s.CustomSidebarForeground).HasMaxLength(32);

        builder.Property(s => s.NavigationPlacement)
            .IsRequired()
            .HasMaxLength(16)
            .HasDefaultValue("sidebar");

        builder.Property(s => s.Radius)
            .IsRequired()
            .HasMaxLength(8)
            .HasDefaultValue("md");

        builder.Property(s => s.CompanyName).HasMaxLength(200);
        builder.Property(s => s.CompanyLogoUrl).HasMaxLength(500);
    }
}
