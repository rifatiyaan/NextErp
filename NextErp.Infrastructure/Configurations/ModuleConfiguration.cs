using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace NextErp.Infrastructure.Configurations
{
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Icon)
                .HasMaxLength(50);

            builder.Property(x => x.Url)
                .HasMaxLength(500);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Version)
                .HasMaxLength(20);

            // Type enum
            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            // Self-referencing relationship
            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // JSON Metadata
            builder.OwnsOne(x => x.Metadata, meta =>
            {
                meta.ToJson();
            });

            // Indexes for common queries
            builder.HasIndex(x => x.Type);
            builder.HasIndex(x => x.ParentId);
            builder.HasIndex(x => new { x.TenantId, x.IsActive });
        }
    }
}
