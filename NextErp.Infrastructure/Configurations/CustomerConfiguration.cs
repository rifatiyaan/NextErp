using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NextErp.Infrastructure.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // Primary key
            builder.HasKey(c => c.Id);
            
            // Ignore legacy navigation
            builder.Ignore(c => c.SalesInvoices);

            // Required fields
            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Email)
                .HasMaxLength(100);

            builder.Property(c => c.Phone)
                .HasMaxLength(20);

            builder.Property(c => c.Address)
                .HasMaxLength(500);

            // JSON column for Metadata
            builder.OwnsOne(c => c.Metadata, meta =>
            {
                meta.ToJson();
            });

            // Indexes
            builder.HasIndex(c => c.Title);
            builder.HasIndex(c => c.IsActive);
            builder.HasIndex(c => c.Email);
        }
    }
}
