using NextErp.Domain.Common;

namespace NextErp.Domain.Entities;

// Invariant: sum(RemainingQuantity) for active batches of a (variant, branch)
// must equal the matching Stock.AvailableQuantity. Maintained by IStockService.
[BranchScoped]
public class StockBatch : IEntity<Guid>, ISoftDeletable
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "Stock Batch";

    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public Guid BranchId { get; set; }
    public Guid TenantId { get; set; }

    public DateTime ReceivedAt { get; set; }

    public decimal OriginalQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }

    public decimal UnitCost { get; set; }

    public Guid? PurchaseItemId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
