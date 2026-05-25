namespace NextErp.Domain.Entities;

/// <summary>
/// One returned line on a <see cref="SaleReturn"/>. We capture the source
/// <see cref="SaleItemId"/> so a partial return knows which original sale
/// row it's against; this matters when the same variant was sold on two
/// separate sale lines at different prices.
/// </summary>
public class SaleReturnItem : IEntity<Guid>
{
    public Guid Id { get; set; }

    /// <summary>IEntity contract requirement. Lines borrow the parent
    /// return's number for display in audit logs.</summary>
    public string Title { get; set; } = string.Empty;

    public Guid SaleReturnId { get; set; }
    public SaleReturn SaleReturn { get; set; } = null!;

    /// <summary>Original sale line being partially or fully returned.</summary>
    public Guid SaleItemId { get; set; }
    public SaleItem SaleItem { get; set; } = null!;

    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;

    /// <summary>Per-line note — e.g. "scratched", "missing-accessory".</summary>
    public string? ConditionNote { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid TenantId { get; set; }
}
