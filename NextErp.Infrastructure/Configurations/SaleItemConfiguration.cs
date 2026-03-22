using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
    {
        public void Configure(EntityTypeBuilder<SaleItem> builder)
        {
            // Primary key
            builder.HasKey(si => si.Id);

            // Required fields
            builder.Property(si => si.Title)
                .IsRequired()
                .HasMaxLength(200);

            // Decimal precision
            builder.Property(si => si.Quantity)
                .HasPrecision(18, 2);

            builder.Property(si => si.Price)
                .HasPrecision(18, 2);

            builder.Property(si => si.UnitPrice)
                .HasPrecision(18, 2);

            // Computed column (Total) - ignored in database
            builder.Ignore(si => si.Total);

            builder.HasOne(si => si.ProductVariant)
                .WithMany()
                .HasForeignKey(si => si.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(si => si.SaleId);
            builder.HasIndex(si => si.ProductVariantId);
        }
    }
}
