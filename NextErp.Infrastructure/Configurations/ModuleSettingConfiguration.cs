using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class ModuleSettingConfiguration : IEntityTypeConfiguration<ModuleSetting>
{
    public void Configure(EntityTypeBuilder<ModuleSetting> builder)
    {
        builder.ToTable("ModuleSettings");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TenantId).IsRequired();
        builder.Property(s => s.Module).IsRequired().HasMaxLength(64);
        builder.Property(s => s.SettingsJson).IsRequired();

        // One row per (tenant, module) — partition key for the JSON blob.
        builder.HasIndex(s => new { s.TenantId, s.Module }).IsUnique();
    }
}
