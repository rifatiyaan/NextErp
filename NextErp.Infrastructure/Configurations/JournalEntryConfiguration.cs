using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id).ValueGeneratedNever();

        builder.Property(j => j.EntryNumber).IsRequired().HasMaxLength(40);
        builder.Property(j => j.Description).IsRequired().HasMaxLength(500);
        builder.Property(j => j.Reference).HasMaxLength(120);
        builder.Property(j => j.Status).HasConversion<int>();
        builder.Property(j => j.ReferenceType).HasConversion<int>();

        // Hot-path lookups: by tenant+branch+date for ledger views, by
        // reference for "find the journal entry behind sale X".
        builder.HasIndex(j => new { j.TenantId, j.BranchId, j.EntryDate });
        builder.HasIndex(j => new { j.ReferenceType, j.ReferenceId });
        builder.HasIndex(j => new { j.TenantId, j.EntryNumber }).IsUnique().HasFilter("[IsActive] = 1");

        builder.HasMany(j => j.Lines)
            .WithOne(l => l.JournalEntry)
            .HasForeignKey(l => l.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Computed properties (TotalDebit/TotalCredit/IsBalanced) live in C#
        // and shouldn't be persisted — EF won't include readonly properties
        // by default but be explicit so a future contributor isn't surprised.
        builder.Ignore(j => j.TotalDebit);
        builder.Ignore(j => j.TotalCredit);
        builder.Ignore(j => j.IsBalanced);
    }
}
