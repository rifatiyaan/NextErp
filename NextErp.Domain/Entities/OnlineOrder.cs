namespace NextErp.Domain.Entities
{
    public enum OnlineOrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2,
    }

    // NOT [BranchScoped]: staff see all branches' online orders.
    public class OnlineOrder : IEntity<int>
    {
        public int Id { get; set; }

        // W + 6-digit tenant-sequential number; the customer-facing reference.
        public string OrderNumber { get; set; } = null!;

        /// <summary>
        /// IEntity contract requirement. We mirror <see cref="OrderNumber"/>
        /// so the title is always derived and never drifts out of sync.
        /// </summary>
        public string Title
        {
            get => OrderNumber;
            set => OrderNumber = value;
        }

        public string CustomerName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? Note { get; set; }

        public OnlineOrderStatus Status { get; set; } = OnlineOrderStatus.Pending;
        public string? CancelReason { get; set; }

        // Snapshot of the flat fee quoted at order time.
        public decimal DeliveryFee { get; set; }

        public Guid? PartyId { get; set; }
        public Party? Party { get; set; }
        public Guid? SaleId { get; set; }
        public Sale? Sale { get; set; }

        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        public ICollection<OnlineOrderItem> Items { get; set; } = new List<OnlineOrderItem>();
    }

    public class OnlineOrderItem : IEntity<int>
    {
        public int Id { get; set; }

        public int OnlineOrderId { get; set; }
        public OnlineOrder OnlineOrder { get; set; } = null!;

        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;

        // Snapshots — exactly what the customer saw and agreed to.
        public string ProductTitle { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal LineTotal { get; set; }

        /// <summary>
        /// IEntity contract requirement. Mirrors <see cref="ProductTitle"/> —
        /// line items never carry a human-meaningful title of their own.
        /// </summary>
        public string Title
        {
            get => ProductTitle;
            set => ProductTitle = value;
        }

        public DateTime CreatedAt { get; set; }
    }
}
