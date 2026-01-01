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
            builder.HasIndex(s => s.ProductId)
                .IsUnique();
        }
    }
}
