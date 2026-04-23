using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Title).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Abbreviation).IsRequired().HasMaxLength(20);
        builder.Property(u => u.Category).HasMaxLength(50);
        builder.Property(u => u.IsSystem).HasDefaultValue(false);
        builder.HasIndex(u => u.Abbreviation).IsUnique();
    }
}
