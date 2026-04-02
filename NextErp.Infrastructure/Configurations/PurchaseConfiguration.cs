using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
            builder.Property(p => p.PurchaseNumber).IsRequired().HasMaxLength(50);

            builder.Property(p => p.TotalAmount).HasPrecision(18, 2);
            builder.Property(p => p.Discount).HasPrecision(18, 2);

            // NetTotal is computed — not stored
            builder.Ignore(p => p.NetTotal);

            // FK to Party (the supplier) — relationship is owned by PartyConfiguration
            builder.HasIndex(p => p.PartyId);
            builder.HasIndex(p => p.PurchaseNumber).IsUnique();
            builder.HasIndex(p => p.PurchaseDate);
            builder.HasIndex(p => p.IsActive);

            builder.HasMany(p => p.Items)
                .WithOne(i => i.Purchase)
                .HasForeignKey(i => i.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsOne(p => p.Metadata, meta => { meta.ToJson(); });
        }
    }
}
