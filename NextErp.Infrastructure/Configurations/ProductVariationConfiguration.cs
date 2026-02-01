using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class ProductVariationConfiguration : IEntityTypeConfiguration<ProductVariation>
    {
        public void Configure(EntityTypeBuilder<ProductVariation> builder)
        {
            builder.Property(pv => pv.PriceAdjustment)
                .HasPrecision(18, 2);

            // Many-to-many relationship with Product
            builder.HasMany(pv => pv.Products)
                .WithMany(p => p.Variations)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductProductVariation",
                    j => j
                        .HasOne<Product>()
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<ProductVariation>()
                        .WithMany()
                        .HasForeignKey("ProductVariationId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("ProductId", "ProductVariationId");
                        j.ToTable("ProductProductVariations");
                    });
        }
    }
}

