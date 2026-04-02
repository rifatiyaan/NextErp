using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
            builder.Property(s => s.SaleNumber).IsRequired().HasMaxLength(50);

            builder.Property(s => s.TotalAmount).HasPrecision(18, 2);
            builder.Property(s => s.Discount).HasPrecision(18, 2);
            builder.Property(s => s.Tax).HasPrecision(18, 2);
            builder.Property(s => s.FinalAmount).HasPrecision(18, 2);

            // FK to Party (the customer) — relationship is owned by PartyConfiguration
            builder.HasIndex(s => s.PartyId);
            builder.HasIndex(s => s.SaleNumber).IsUnique();
            builder.HasIndex(s => s.SaleDate);
            builder.HasIndex(s => s.IsActive);

            builder.HasMany(s => s.Items)
                .WithOne(i => i.Sale)
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsOne(s => s.Metadata, meta => { meta.ToJson(); });
        }
    }
}
