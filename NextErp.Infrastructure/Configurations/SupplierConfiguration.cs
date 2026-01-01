using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            // Primary key
            builder.HasKey(s => s.Id);
            
            // Ignore legacy navigation
            builder.Ignore(s => s.PurchaseInvoices);

            // Required fields
            builder.Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.ContactPerson)
                .HasMaxLength(100);

            builder.Property(s => s.Phone)
                .HasMaxLength(20);

            builder.Property(s => s.Email)
                .HasMaxLength(100);

            builder.Property(s => s.Address)
                .HasMaxLength(500);

            // JSON column for Metadata
            builder.OwnsOne(s => s.Metadata, meta =>
            {
                meta.ToJson();
            });

            // Indexes
            builder.HasIndex(s => s.Title);
            builder.HasIndex(s => s.IsActive);
        }
    }
}
