using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            // Primary key
            builder.HasKey(s => s.Id);

            // Required fields
            builder.Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.SaleNumber)
                .IsRequired()
                .HasMaxLength(50);

            // Decimal precision
            builder.Property(s => s.TotalAmount)
                .HasPrecision(18, 2);

            // Relationship with Customer
            builder.HasOne(s => s.Customer)
                .WithMany() // Customer has collection navigation in entity
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship with SaleItems
            builder.HasMany(s => s.Items)
                .WithOne(i => i.Sale)
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            // JSON column for Metadata
            builder.OwnsOne(s => s.Metadata, meta =>
            {
                meta.ToJson();
            });

            // Indexes
            builder.HasIndex(s => s.SaleNumber)
                .IsUnique();
            builder.HasIndex(s => s.CustomerId);
            builder.HasIndex(s => s.SaleDate);
            builder.HasIndex(s => s.IsActive);
        }
    }
}
