using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations
{
    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(b => b.Address)
                .HasMaxLength(500);

            builder.OwnsOne(b => b.Metadata, meta =>
            {
                meta.ToJson();
            });

            builder.HasIndex(b => b.Title);
            builder.HasIndex(b => new { b.TenantId, b.IsActive });
        }
    }
}
