using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class StockBatchConfiguration : IEntityTypeConfiguration<StockBatch>
{
    public void Configure(EntityTypeBuilder<StockBatch> builder)
    {
        builder.ToTable("StockBatches");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.OriginalQuantity).HasPrecision(18, 4);
        builder.Property(b => b.RemainingQuantity).HasPrecision(18, 4);
        builder.Property(b => b.UnitCost).HasPrecision(18, 4);

        builder.HasOne(b => b.ProductVariant)
            .WithMany()
            .HasForeignKey(b => b.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Hot-path index: load remaining batches for a variant + branch in
        // FIFO/LIFO order. Filter on RemainingQuantity > 0 keeps the index
        // narrow as exhausted batches accumulate.
        builder.HasIndex(b => new { b.ProductVariantId, b.BranchId, b.ReceivedAt })
            .HasFilter("[RemainingQuantity] > 0")
            .HasDatabaseName("IX_StockBatches_Open");
    }
}
