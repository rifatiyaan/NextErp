using NextErp.Domain.Entities;

namespace NextErp.Application.Services;

/// <summary>
/// In-memory snapshot of variants + their tracked Stock rows for a single branch,
/// used by handlers that perform multiple stock movements in one transaction
/// (e.g. CreateSale, CreatePurchase). All entities are EF-tracked, so mutations
/// to Stock.AvailableQuantity flush on SaveChangesAsync without further queries.
/// </summary>
public sealed class StockContext
{
    public Guid BranchId { get; }
    public Guid TenantId { get; }
    public IReadOnlyDictionary<int, ProductVariant> Variants { get; }

    // Mutable so the StockService can register newly-created Stock rows
    // when a positive movement targets a variant with no row yet.
    private readonly Dictionary<int, Stock> _stocks;

    public StockContext(
        Guid branchId,
        Guid tenantId,
        IReadOnlyDictionary<int, ProductVariant> variants,
        Dictionary<int, Stock> stocks)
    {
        BranchId = branchId;
        TenantId = tenantId;
        Variants = variants;
        _stocks = stocks;
    }

    public ProductVariant GetVariant(int productVariantId) =>
        Variants.TryGetValue(productVariantId, out var variant)
            ? variant
            : throw new InvalidOperationException(
                $"Product variant {productVariantId} is not loaded in this StockContext.");

    public Stock? GetStockOrNull(int productVariantId) =>
        _stocks.TryGetValue(productVariantId, out var stock) ? stock : null;

    internal void RegisterStock(Stock stock)
    {
        _stocks[stock.ProductVariantId] = stock;
    }
}
