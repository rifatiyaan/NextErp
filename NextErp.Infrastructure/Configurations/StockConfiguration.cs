using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            // Primary key
            builder.HasKey(s => s.Id);

            // Configure Id to NOT be an IDENTITY column (so we can set it to ProductId)
            builder.Property(s => s.Id)
                .ValueGeneratedNever(); // Don't auto-generate, we'll set it manually

            // ProductId is the same as Id (one-to-one with Product)
            builder.Property(s => s.ProductId)
                .IsRequired();

            // One-to-one relationship with Product
            builder.HasOne(s => s.Product)
                .WithMany() // Product doesn't have navigation to Stock
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // RowVersion as concurrency token
            builder.Property(s => s.RowVersion)
                .IsRowVersion()
                .IsRequired();

            // Decimal precision
            builder.Property(s => s.AvailableQuantity)
                .HasPrecision(18, 2);

            // Indexes
            // Unique constraint: one Stock per Product (for single warehouse)
            // To support multi-warehouse: remove .IsUnique() and add composite index on (ProductId, WarehouseId)
            builder.HasIndex(s => s.ProductId)
                .IsUnique();
        }
    }
}
