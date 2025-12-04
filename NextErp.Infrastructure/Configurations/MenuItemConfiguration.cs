using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace NextErp.Infrastructure.Configurations
{
    public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Icon)
                .HasMaxLength(50);

            builder.Property(x => x.Url)
                .HasMaxLength(500);

            // Self-referencing relationship
            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Module relationship
            builder.HasOne(x => x.Module)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.ModuleId)
                .OnDelete(DeleteBehavior.SetNull);

            // JSON Metadata
            builder.Property(x => x.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<MenuItem.MenuItemMetadata>(v, (JsonSerializerOptions?)null)!)
                .HasColumnType("nvarchar(max)");
        }
    }
}
