using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class VariationValueConfiguration : IEntityTypeConfiguration<VariationValue>
    {
        public void Configure(EntityTypeBuilder<VariationValue> builder)
        {
            // VariationOption relationship
            builder.HasOne(vv => vv.VariationOption)
                .WithMany(vo => vo.Values)
                .HasForeignKey(vv => vv.VariationOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many relationship with ProductVariant
            builder.HasMany(vv => vv.ProductVariants)
                .WithMany(pv => pv.VariationValues)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductVariantVariationValue",
                    j => j
                        .HasOne<ProductVariant>()
                        .WithMany()
                        .HasForeignKey("ProductVariantId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<VariationValue>()
                        .WithMany()
                        .HasForeignKey("VariationValueId")
                        .OnDelete(DeleteBehavior.NoAction), // Changed to NoAction to avoid multiple cascade paths
                    j =>
                    {
                        j.HasKey("ProductVariantId", "VariationValueId");
                        j.ToTable("ProductVariantVariationValues");
                    });

            // Index for performance
            builder.HasIndex(vv => vv.VariationOptionId);
        }
    }
}

