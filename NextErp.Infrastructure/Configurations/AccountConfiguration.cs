using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Code).IsRequired().HasMaxLength(32);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(120);
        builder.Property(a => a.Description).HasMaxLength(500);
        builder.Property(a => a.Type).HasConversion<int>();

        // CoA codes are unique per tenant. Filtered index excludes soft-deleted
        // rows so re-creating an account with a freed code stays possible.
        builder.HasIndex(a => new { a.TenantId, a.Code })
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        builder.HasOne(a => a.ParentAccount)
            .WithMany(a => a.Children)
            .HasForeignKey(a => a.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
