namespace NextErp.Domain.Entities
{
    /// <summary>
    /// Server-side record of an issued refresh token. We store only a SHA-256
    /// hash of the token (never the raw value) so a database leak yields nothing
    /// usable — the same reasoning as password hashing. Rotation is tracked via
    /// <see cref="ReplacedByTokenHash"/> so a re-used (already-rotated) token can
    /// be detected as theft and the whole chain revoked.
    /// </summary>
    public class RefreshToken
    {
        public Guid Id { get; set; }

        // FK to ApplicationUser (AspNetUsers.Id is a Guid). Plain column + index;
        // no navigation, which is what kept the old scaffold pointing at a phantom
        // string-keyed "IdentityUser" table.
        public Guid UserId { get; set; }

        // SHA-256(raw token) as lowercase hex. Unique per issued token.
        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Set when the token is rotated out (on /refresh) or revoked (on /logout).
        public DateTime? RevokedAt { get; set; }

        // The hash of the token that replaced this one during rotation. Lets us
        // detect reuse of an already-rotated token.
        public string? ReplacedByTokenHash { get; set; }

        // Usable only while not revoked and not expired.
        public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
    }
}
