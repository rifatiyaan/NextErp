using NextErp.Domain.Common;

namespace NextErp.Domain.Entities
{
    public enum PartyType
    {
        Customer = 0,
        Supplier = 1,
        User = 2
    }

    [BranchScoped]
    public class Party : IEntity<Guid>, ISoftDeletable
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        // Merged from Customer
        public string? LoyaltyCode { get; set; }
        public string? NationalId { get; set; }

        /// <summary>
        /// Membership tier label (e.g. "silver", "gold", "platinum"). Joins
        /// the Promotion entity's <c>Config.MembershipTier</c> field — when
        /// a sale is rung up for a party with a matching tier, the engine
        /// auto-applies that promotion.
        /// </summary>
        public string? MembershipTier { get; set; }

        // Merged from Supplier
        public string? ContactPerson { get; set; }
        public string? VatNumber { get; set; }
        public string? TaxId { get; set; }

        public string? Notes { get; set; }

        public PartyType PartyType { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }

        // Navigation: one-to-many (this Party as customer of sales)
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();

        // Navigation: one-to-many (this Party as supplier of purchases)
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

        // Navigation: optional linked user account
        public ApplicationUser? User { get; set; }
    }
}
