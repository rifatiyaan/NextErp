using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class SaleReturnConfiguration : IEntityTypeConfiguration<SaleReturn>
{
    public void Configure(EntityTypeBuilder<SaleReturn> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReturnNumber).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Reason).HasMaxLength(200);
        builder.Property(r => r.Notes).HasMaxLength(1000);
        builder.Property(r => r.TotalRefund).HasPrecision(18, 2);

        // Title is a get/set mirror of ReturnNumber that exists only to
        // satisfy the IEntity<Guid> contract. We don't want EF to materialise
        // it as a separate column — that would store the same value twice
        // and let the two drift if anything ever bypassed the property.
        builder.Ignore(r => r.Title);

        builder.HasIndex(r => r.ReturnNumber).IsUnique();
        builder.HasIndex(r => r.SaleId);
        builder.HasIndex(r => r.ReturnDate);
        builder.HasIndex(r => r.IsActive);

        builder.HasOne(r => r.Sale)
            .WithMany()
            .HasForeignKey(r => r.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Items)
            .WithOne(i => i.SaleReturn)
            .HasForeignKey(i => i.SaleReturnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
