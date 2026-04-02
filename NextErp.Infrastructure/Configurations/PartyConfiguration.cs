using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations
{
    public class PartyConfiguration : IEntityTypeConfiguration<Party>
    {
        public void Configure(EntityTypeBuilder<Party> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
            builder.Property(p => p.FirstName).HasMaxLength(100);
            builder.Property(p => p.LastName).HasMaxLength(100);
            builder.Property(p => p.Email).HasMaxLength(256);
            builder.Property(p => p.Phone).HasMaxLength(50);
            builder.Property(p => p.Address).HasMaxLength(500);
            builder.Property(p => p.ContactPerson).HasMaxLength(200);
            builder.Property(p => p.LoyaltyCode).HasMaxLength(100);
            builder.Property(p => p.NationalId).HasMaxLength(100);
            builder.Property(p => p.VatNumber).HasMaxLength(100);
            builder.Property(p => p.TaxId).HasMaxLength(100);
            builder.Property(p => p.Notes).HasMaxLength(2000);
            builder.Property(p => p.PartyType).IsRequired();

            // One Party can be linked to one ApplicationUser
            builder.HasOne(p => p.User)
                .WithOne(u => u.Party)
                .HasForeignKey<ApplicationUser>(u => u.PartyId)
                .OnDelete(DeleteBehavior.SetNull);

            // One Party (Customer) can have many Sales
            builder.HasMany(p => p.Sales)
                .WithOne(s => s.Party)
                .HasForeignKey(s => s.PartyId)
                .OnDelete(DeleteBehavior.SetNull);

            // One Party (Supplier) can have many Purchases
            builder.HasMany(p => p.Purchases)
                .WithOne(p => p.Party)
                .HasForeignKey(p => p.PartyId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(p => p.PartyType);
            builder.HasIndex(p => p.BranchId);
            builder.HasIndex(p => p.Email);
            builder.HasIndex(p => p.TenantId);
        }
    }
}
