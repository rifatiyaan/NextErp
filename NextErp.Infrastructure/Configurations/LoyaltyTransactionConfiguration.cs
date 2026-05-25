using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class LoyaltyTransactionConfiguration : IEntityTypeConfiguration<LoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
    {
        builder.ToTable("LoyaltyTransactions");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.Reason).HasConversion<int>();
        builder.Property(l => l.Notes).HasMaxLength(500);
        builder.Property(l => l.ReferenceType).HasMaxLength(64);

        // Per-customer balance lookup is the hot path (sum points where
        // CustomerId = X, IsActive = 1). Compound index keeps it sargable.
        builder.HasIndex(l => new { l.TenantId, l.CustomerId, l.IsActive });
        builder.HasIndex(l => l.CreatedAt);
    }
}
