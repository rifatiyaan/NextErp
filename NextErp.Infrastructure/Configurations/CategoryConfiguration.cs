using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace NextErp.Infrastructure.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            // Category self-referencing (Parent -> Children)
            builder.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // JSON column for Category.Metadata (EF Core 8+ syntax)
            builder.OwnsOne(c => c.Metadata, meta =>
            {
                meta.ToJson();
            });
        }
    }
}
