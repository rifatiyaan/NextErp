using NextErp.Domain.Common;

namespace NextErp.Domain.Entities;

/// <summary>
/// Customer-initiated return of part (or all) of a Sale. Returns are
/// committed at creation time — there's no Draft state — because the
/// receiver typically inspects the goods first, and once they're back on
/// the shelf the stock has already moved.
///
/// Each row is paired with one <see cref="SaleReturnItem"/> per returned
/// line. The handler creates a <see cref="StockMovement"/> with positive
/// delta and <see cref="StockMovementType.Return"/> at the same time so
/// the inventory total stays in sync.
/// </summary>
[BranchScoped]
public class SaleReturn : IEntity<Guid>, ISoftDeletable
{
    public Guid Id { get; set; }

    /// <summary>"RET-S-YYYYMMDD-XXXXXXXX" generated server-side.</summary>
    public string ReturnNumber { get; set; } = null!;

    /// <summary>
    /// IEntity contract requirement. We mirror <see cref="ReturnNumber"/>
    /// so the title is always derived — saves us from having to set/update
    /// two fields whenever a return is renumbered.
    /// </summary>
    public string Title
    {
        get => ReturnNumber;
        set => ReturnNumber = value;
    }

    /// <summary>The Sale this return is against. Required.</summary>
    public Guid SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public DateTime ReturnDate { get; set; }

    /// <summary>Short reason code — "damaged", "wrong-item", "customer-changed-mind", etc.</summary>
    public string? Reason { get; set; }

    /// <summary>Free-form operator note.</summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Sum of (Quantity × UnitPrice) across all <see cref="Items"/>. We
    /// snapshot the refund total at create time rather than re-deriving it
    /// at read time so a back-end change to per-line pricing math can't
    /// silently restate a closed return.
    /// </summary>
    public decimal TotalRefund { get; set; }

    public ICollection<SaleReturnItem> Items { get; set; } = new List<SaleReturnItem>();

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
}
