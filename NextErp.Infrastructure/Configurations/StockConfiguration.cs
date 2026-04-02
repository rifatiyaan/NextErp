using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedNever();

            builder.HasOne(s => s.ProductVariant)
                .WithMany(pv => pv.StockRecords)
                .HasForeignKey(s => s.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(s => s.RowVersion)
                .IsRowVersion()
                .IsRequired();

            builder.Property(s => s.AvailableQuantity)
                .HasPrecision(18, 2);

            builder.Property(s => s.BranchId)
                .IsRequired();

            builder.HasIndex(s => new { s.ProductVariantId, s.BranchId })
                .IsUnique();

            builder.HasIndex(s => s.ProductVariantId);
            builder.HasIndex(s => s.BranchId);
        }
    }
}
