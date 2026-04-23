using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            // Price precision
            builder.Property(pv => pv.Price)
                .HasPrecision(18, 2);

            // Product relationship
            builder.HasOne(pv => pv.Product)
                .WithMany(p => p.ProductVariants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique SKU constraint per product
            builder.HasIndex(pv => new { pv.ProductId, pv.Sku })
                .IsUnique();

            // Indexes for performance
            builder.HasIndex(pv => pv.ProductId);
            builder.HasIndex(pv => pv.Sku);
        }
    }
}

