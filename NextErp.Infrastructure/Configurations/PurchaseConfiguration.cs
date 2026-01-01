using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            // Primary key
            builder.HasKey(p => p.Id);

            // Required fields
            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.PurchaseNumber)
                .IsRequired()
                .HasMaxLength(50);

            // Decimal precision
            builder.Property(p => p.TotalAmount)
                .HasPrecision(18, 2);

            // Relationship with Supplier
            builder.HasOne(p => p.Supplier)
                .WithMany() // Supplier has collection navigation in entity
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship with PurchaseItems
            builder.HasMany(p => p.Items)
                .WithOne(i => i.Purchase)
                .HasForeignKey(i => i.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // JSON column for Metadata
            builder.OwnsOne(p => p.Metadata, meta =>
            {
                meta.ToJson();
            });

            // Indexes
            builder.HasIndex(p => p.PurchaseNumber)
                .IsUnique();
            builder.HasIndex(p => p.SupplierId);
            builder.HasIndex(p => p.PurchaseDate);
            builder.HasIndex(p => p.IsActive);
        }
    }
}
