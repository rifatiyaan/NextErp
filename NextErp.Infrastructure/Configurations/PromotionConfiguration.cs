using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Type).HasConversion<int>();

        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => new { p.StartDate, p.EndDate });

        // PromotionConfig is owned + serialised as JSON. Same pattern as
        // SaleMetadata/PurchaseMetadata — keeps the schema single-table
        // while allowing arbitrary type-specific fields per row.
        builder.OwnsOne(p => p.Config, c =>
        {
            c.ToJson();
            c.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            c.Property(x => x.DiscountPercent).HasPrecision(5, 2);
            c.Property(x => x.MinSubtotal).HasPrecision(18, 2);
            c.Property(x => x.BuyQuantity).HasPrecision(18, 2);
            c.Property(x => x.GetQuantity).HasPrecision(18, 2);
            c.Property(x => x.GetDiscountPercent).HasPrecision(5, 2);
            c.Property(x => x.MembershipTier).HasMaxLength(50);
        });
    }
}
