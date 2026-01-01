using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseItem> builder)
        {
            // Primary key
            builder.HasKey(pi => pi.Id);

            // Required fields
            builder.Property(pi => pi.Title)
                .IsRequired()
                .HasMaxLength(200);

            // Decimal precision
            builder.Property(pi => pi.Quantity)
                .HasPrecision(18, 2);

            builder.Property(pi => pi.UnitCost)
                .HasPrecision(18, 2);

            // Computed column (Total) - ignored in database
            builder.Ignore(pi => pi.Total);

            // Relationship with Product
            builder.HasOne(pi => pi.Product)
                .WithMany()
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(pi => pi.PurchaseId);
            builder.HasIndex(pi => pi.ProductId);
        }
    }
}
