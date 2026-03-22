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

            // Metadata as JSON column
            builder.Property(pi => pi.Metadata)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<PurchaseItem.PurchaseItemMetadata>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new PurchaseItem.PurchaseItemMetadata()
                )
                .HasColumnType("nvarchar(max)");

            builder.HasOne(pi => pi.ProductVariant)
                .WithMany()
                .HasForeignKey(pi => pi.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pi => pi.PurchaseId);
            builder.HasIndex(pi => pi.ProductVariantId);
        }
    }
}
