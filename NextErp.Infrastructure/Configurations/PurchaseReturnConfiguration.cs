using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReturnNumber).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Reason).HasMaxLength(200);
        builder.Property(r => r.Notes).HasMaxLength(1000);
        builder.Property(r => r.TotalAmount).HasPrecision(18, 2);

        // Title mirrors ReturnNumber via a get/set property; see
        // SaleReturnConfiguration for the same rationale.
        builder.Ignore(r => r.Title);

        builder.HasIndex(r => r.ReturnNumber).IsUnique();
        builder.HasIndex(r => r.PurchaseId);
        builder.HasIndex(r => r.ReturnDate);
        builder.HasIndex(r => r.IsActive);

        builder.HasOne(r => r.Purchase)
            .WithMany()
            .HasForeignKey(r => r.PurchaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Items)
            .WithOne(i => i.PurchaseReturn)
            .HasForeignKey(i => i.PurchaseReturnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
