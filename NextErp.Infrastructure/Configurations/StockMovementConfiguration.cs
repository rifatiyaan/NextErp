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

        builder.Property(m => m.QuantityChanged).HasPrecision(18, 2);
        builder.Property(m => m.PreviousQuantity).HasPrecision(18, 2);
        builder.Property(m => m.NewQuantity).HasPrecision(18, 2);

        builder.Property(m => m.MovementType)
            .HasColumnName("Type")
            .HasConversion<int>();

        builder.HasOne(m => m.Stock)
            .WithMany(s => s.Movements)
            .HasForeignKey(m => m.StockId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.ProductVariant)
            .WithMany()
            .HasForeignKey(m => m.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.BranchId).IsRequired();
        builder.HasIndex(m => m.BranchId);
        builder.HasIndex(m => m.StockId);
        builder.HasIndex(m => new { m.ProductVariantId, m.BranchId, m.CreatedAt });
        builder.HasIndex(m => m.ReferenceId);
    }
}

