using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class PurchaseReturnItemConfiguration : IEntityTypeConfiguration<PurchaseReturnItem>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Quantity).HasPrecision(18, 2);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.ConditionNote).HasMaxLength(500);

        builder.Ignore(i => i.Subtotal);
        // See SaleReturnItemConfiguration for the same Title-ignore rationale.
        builder.Ignore(i => i.Title);

        builder.HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.PurchaseItem)
            .WithMany()
            .HasForeignKey(i => i.PurchaseItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.PurchaseReturnId);
        builder.HasIndex(i => i.ProductVariantId);
    }
}
