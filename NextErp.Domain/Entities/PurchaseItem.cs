namespace NextErp.Domain.Entities
{
    public class PurchaseItem : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;

        public Guid PurchaseId { get; set; }
        public Purchase Purchase { get; set; } = null!;

        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }

        /// <summary>
        /// Final $ amount discounted from this line. Purchase side accepts
        /// manual operator entries only (no rule engine on purchases in
        /// MVP). LineTotal = Quantity*UnitCost - Discount.
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>Audit hint — Manual is the only valid value on purchase side for MVP.</summary>
        public DiscountSource? DiscountSource { get; set; }

        public decimal Total => Quantity * UnitCost - Discount;

        public PurchaseItemMetadata Metadata { get; set; } = new PurchaseItemMetadata();

        public DateTime CreatedAt { get; set; }

        public Guid TenantId { get; set; }

        public class PurchaseItemMetadata
        {
            public string? Description { get; set; }
            public decimal? Weight { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public string? BatchNumber { get; set; }
        }
    }
}
