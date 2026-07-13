using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        // SHA-256 hex is 64 chars; cap and index for fast lookup on /refresh.
        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(128);
        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(128);

        // Look up + revoke all of a user's tokens (logout / sign-out-everywhere).
        builder.HasIndex(t => t.UserId);

        // Not a mapped navigation on purpose — just a scalar FK so EF never
        // materialises a second "IdentityUser" table (the old scaffold's bug).
        // Cascade so deleting a user cleans up their refresh tokens.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(t => t.IsActive);
    }
}
