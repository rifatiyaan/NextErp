using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class VariationOptionConfiguration : IEntityTypeConfiguration<VariationOption>
    {
        public void Configure(EntityTypeBuilder<VariationOption> builder)
        {
            // Product relationship
            builder.HasOne(vo => vo.Product)
                .WithMany(p => p.VariationOptions)
                .HasForeignKey(vo => vo.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for performance
            builder.HasIndex(vo => vo.ProductId);
        }
    }
}

