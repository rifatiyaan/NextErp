using NextErp.Domain.Common;

namespace NextErp.Domain.Entities
{
    [BranchScoped]
    public class Sale : IEntity<Guid>, ISoftDeletable
    {
        public const decimal DefaultTaxRate = 0.05m;

        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string SaleNumber { get; set; } = null!;

        public Guid? PartyId { get; set; }
        public Party? Party { get; set; }

        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; } = 0;
        public decimal Tax { get; set; } = 0;
        public decimal FinalAmount { get; set; }

        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();

        public ICollection<SalePayment> Payments { get; set; } = new List<SalePayment>();

        public SaleMetadata Metadata { get; set; } = new SaleMetadata();

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }

        public class SaleMetadata
        {
            public string? ReferenceNo { get; set; }
            public string? PaymentMethod { get; set; }
            public string? Notes { get; set; }
        }

        /// <summary>Non-negative discount clamp.</summary>
        public static decimal NormalizeDiscount(decimal discountRequested) =>
            discountRequested < 0 ? 0 : discountRequested;

        public static decimal CalculateTax(decimal grossTotal, decimal taxRate)
        {
            if (grossTotal < 0)
                throw new ArgumentOutOfRangeException(nameof(grossTotal), "Gross total cannot be negative.");
            if (taxRate < 0)
                throw new ArgumentOutOfRangeException(nameof(taxRate), "Tax rate cannot be negative.");

            return decimal.Round(grossTotal * taxRate, 2, MidpointRounding.AwayFromZero);
        }

        public static decimal CalculateFinalAmount(decimal grossTotal, decimal tax, decimal discount)
        {
            var final = decimal.Round(grossTotal + tax - discount, 2, MidpointRounding.AwayFromZero);
            return final < 0 ? 0 : final;
        }

        /// <summary>Creates a sale header with totals derived from line gross, discount, and tax rate.</summary>
        public static Sale Create(
            Guid id,
            string saleNumber,
            string title,
            Guid tenantId,
            Guid branchId,
            Guid? partyId,
            decimal itemsGrossTotal,
            decimal discountRequested,
            decimal taxRate,
            DateTime saleDate,
            SaleMetadata? metadata)
        {
            if (itemsGrossTotal < 0)
                throw new ArgumentOutOfRangeException(nameof(itemsGrossTotal), "Items gross total cannot be negative.");

            var discount = NormalizeDiscount(discountRequested);
            var tax = CalculateTax(itemsGrossTotal, taxRate);
            var finalAmount = CalculateFinalAmount(itemsGrossTotal, tax, discount);

            return new Sale
            {
                Id = id,
                Title = title,
                SaleNumber = saleNumber,
                PartyId = partyId,
                SaleDate = saleDate,
                TotalAmount = itemsGrossTotal,
                Discount = discount,
                Tax = tax,
                FinalAmount = finalAmount,
                Metadata = metadata ?? new SaleMetadata(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = null,
                TenantId = tenantId,
                BranchId = branchId,
                Items = new List<SaleItem>(),
                Payments = new List<SalePayment>()
            };
        }
    }
}
