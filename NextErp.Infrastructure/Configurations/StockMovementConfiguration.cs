using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Quantity).HasPrecision(18, 2);

        builder.HasOne(m => m.ProductVariant)
            .WithMany()
            .HasForeignKey(m => m.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.BranchId).IsRequired();
        builder.HasIndex(m => m.BranchId);
        builder.HasIndex(m => new { m.ProductVariantId, m.BranchId, m.CreatedAt });
        builder.HasIndex(m => m.ReferenceId);
    }
}
