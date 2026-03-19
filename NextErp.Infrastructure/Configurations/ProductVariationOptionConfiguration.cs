using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations
{
    public class ProductVariationOptionConfiguration : IEntityTypeConfiguration<ProductVariationOption>
    {
        public void Configure(EntityTypeBuilder<ProductVariationOption> builder)
        {
            builder.HasOne(pvo => pvo.Product)
                .WithMany(p => p.ProductVariationOptions)
                .HasForeignKey(pvo => pvo.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pvo => pvo.VariationOption)
                .WithMany(vo => vo.ProductVariationOptions)
                .HasForeignKey(pvo => pvo.VariationOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pvo => pvo.ProductId);
            builder.HasIndex(pvo => pvo.VariationOptionId);
            builder.HasIndex(pvo => new { pvo.ProductId, pvo.VariationOptionId }).IsUnique();
        }
    }
}
