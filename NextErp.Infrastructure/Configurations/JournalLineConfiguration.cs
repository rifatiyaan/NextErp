using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class JournalLineConfiguration : IEntityTypeConfiguration<JournalLine>
{
    public void Configure(EntityTypeBuilder<JournalLine> builder)
    {
        builder.ToTable("JournalLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.Description).HasMaxLength(500);
        builder.Property(l => l.Debit).HasColumnType("decimal(18,4)");
        builder.Property(l => l.Credit).HasColumnType("decimal(18,4)");

        // Account-side lookup ("show me every line that hit account X") is
        // the dominant query for ledger views, so index AccountId.
        builder.HasIndex(l => l.AccountId);
        builder.HasIndex(l => l.JournalEntryId);

        builder.HasOne(l => l.Account)
            .WithMany(a => a.JournalLines)
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
