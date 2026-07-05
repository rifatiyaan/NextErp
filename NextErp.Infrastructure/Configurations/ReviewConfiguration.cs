using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.AuthorName).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Text).HasMaxLength(2000).IsRequired();

        // Title is a get/set mirror of AuthorName that exists only to satisfy
        // the IEntity<int> contract; don't materialise it as its own column.
        builder.Ignore(r => r.Title);

        // Storefront reads approved reviews for one product, newest first.
        builder.HasIndex(r => new { r.ProductId, r.IsApproved });

        builder.HasOne(r => r.Product).WithMany().HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
