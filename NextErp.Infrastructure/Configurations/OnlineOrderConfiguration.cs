using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class OnlineOrderConfiguration : IEntityTypeConfiguration<OnlineOrder>
{
    public void Configure(EntityTypeBuilder<OnlineOrder> builder)
    {
        builder.Property(o => o.OrderNumber).HasMaxLength(16).IsRequired();
        builder.HasIndex(o => new { o.TenantId, o.OrderNumber }).IsUnique();

        // Title is a get/set mirror of OrderNumber that exists only to
        // satisfy the IEntity<int> contract; see SaleReturnConfiguration
        // for the same rationale. Don't materialise it as its own column.
        builder.Ignore(o => o.Title);
        builder.Property(o => o.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Phone).HasMaxLength(32).IsRequired();
        builder.Property(o => o.Address).HasMaxLength(1000).IsRequired();
        builder.Property(o => o.Note).HasMaxLength(1000);
        builder.Property(o => o.CancelReason).HasMaxLength(500);
        builder.Property(o => o.DeliveryFee).HasPrecision(18, 2);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.OnlineOrder)
            .HasForeignKey(i => i.OnlineOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Party).WithMany().HasForeignKey(o => o.PartyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Sale).WithMany().HasForeignKey(o => o.SaleId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class OnlineOrderItemConfiguration : IEntityTypeConfiguration<OnlineOrderItem>
{
    public void Configure(EntityTypeBuilder<OnlineOrderItem> builder)
    {
        builder.Property(i => i.ProductTitle).HasMaxLength(300).IsRequired();
        builder.Property(i => i.Sku).HasMaxLength(64).IsRequired();

        // Title mirrors ProductTitle; see SaleReturnItemConfiguration for
        // the same Title-ignore rationale.
        builder.Ignore(i => i.Title);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.Quantity).HasPrecision(18, 3);
        builder.Property(i => i.LineTotal).HasPrecision(18, 2);

        builder.HasOne(i => i.ProductVariant).WithMany().HasForeignKey(i => i.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
    }
}
