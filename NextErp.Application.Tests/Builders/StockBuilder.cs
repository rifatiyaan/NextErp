using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Builders;

public class StockBuilder
{
    private Guid _id = Guid.NewGuid();
    private int _productVariantId = 1;
    private decimal _availableQuantity;
    private decimal? _reorderLevel;
    private Guid _tenantId = Guid.NewGuid();
    private Guid _branchId = Guid.NewGuid();

    public StockBuilder WithVariant(int productVariantId) { _productVariantId = productVariantId; return this; }
    public StockBuilder WithAvailable(decimal qty) { _availableQuantity = qty; return this; }
    public StockBuilder WithReorderLevel(decimal level) { _reorderLevel = level; return this; }
    public StockBuilder WithTenant(Guid tenantId) { _tenantId = tenantId; return this; }
    public StockBuilder WithBranch(Guid branchId) { _branchId = branchId; return this; }

    public Stock Build() => new()
    {
        Id = _id,
        Title = "Stock",
        ProductVariantId = _productVariantId,
        AvailableQuantity = _availableQuantity,
        ReorderLevel = _reorderLevel,
        TenantId = _tenantId,
        BranchId = _branchId,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        // SQLite has no native rowversion concept; supply a non-null value so the column
        // (configured as IsRequired + IsRowVersion) doesn't trip on insert.
        RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 },
    };
}
