namespace NextErp.Domain.Entities
{
    public class SaleItem : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;

        public Guid SaleId { get; set; }
        public Sale Sale { get; set; } = null!;

        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal Price { get; set; }

        /// <summary>
        /// Final $ amount discounted from this line (after manual + rule
        /// engine resolution). LineTotal = Quantity*Price - Discount.
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>Audit hint — was the discount typed by the operator or auto-applied?</summary>
        public DiscountSource? DiscountSource { get; set; }

        /// <summary>Link to the Promotion that contributed the discount, if any.</summary>
        public Guid? PromotionId { get; set; }

        // NULL when not tracked (Single mode or pre-batch-ledger sale).
        public decimal? UnitCostAtSale { get; set; }

        public decimal Subtotal => Quantity * Price - Discount;

        // Legacy support
        public decimal UnitPrice { get => Price; set => Price = value; }
        public decimal Total => Subtotal;

        public DateTime CreatedAt { get; set; }

        public Guid TenantId { get; set; }
    }
}
