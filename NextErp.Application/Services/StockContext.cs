using NextErp.Domain.Entities;

namespace NextErp.Application.Services;

public sealed record BatchConsumption(Guid BatchId, decimal Quantity, decimal UnitCost);

public sealed class StockContext
{
    public Guid BranchId { get; }
    public Guid TenantId { get; }
    public IReadOnlyDictionary<int, ProductVariant> Variants { get; }

    // Mutable so the StockService can register newly-created Stock rows
    // when a positive movement targets a variant with no row yet.
    private readonly Dictionary<int, Stock> _stocks;

    // Open batches keyed by variantId, each list sorted ASC by ReceivedAt at
    // load time. Mutated in-place as the handler creates/consumes batches.
    private readonly Dictionary<int, List<StockBatch>> _openBatches;

    public StockContext(
        Guid branchId,
        Guid tenantId,
        IReadOnlyDictionary<int, ProductVariant> variants,
        Dictionary<int, Stock> stocks,
        Dictionary<int, List<StockBatch>>? openBatches = null)
    {
        BranchId = branchId;
        TenantId = tenantId;
        Variants = variants;
        _stocks = stocks;
        _openBatches = openBatches ?? new Dictionary<int, List<StockBatch>>();
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

    // List is maintained ASC by ReceivedAt so FIFO walks stay trivial.
    internal List<StockBatch> GetOpenBatches(int productVariantId)
    {
        if (_openBatches.TryGetValue(productVariantId, out var list))
            return list;
        var fresh = new List<StockBatch>();
        _openBatches[productVariantId] = fresh;
        return fresh;
    }

    internal void RegisterBatch(StockBatch batch)
    {
        var list = GetOpenBatches(batch.ProductVariantId);
        var insertAt = list.FindIndex(b => b.ReceivedAt > batch.ReceivedAt);
        if (insertAt < 0) list.Add(batch);
        else list.Insert(insertAt, batch);
    }
}

