using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class SaleReturnItemConfiguration : IEntityTypeConfiguration<SaleReturnItem>
{
    public void Configure(EntityTypeBuilder<SaleReturnItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Quantity).HasPrecision(18, 2);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.ConditionNote).HasMaxLength(500);

        builder.Ignore(i => i.Subtotal);
        // Title is purely an IEntity<Guid> contract requirement — line items
        // never carry a human-meaningful title of their own; the parent
        // return's number is enough for display.
        builder.Ignore(i => i.Title);

        builder.HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.SaleItem)
            .WithMany()
            .HasForeignKey(i => i.SaleItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.SaleReturnId);
        builder.HasIndex(i => i.ProductVariantId);
    }
}
