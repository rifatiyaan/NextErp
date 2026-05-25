using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Settings;
using NextErp.Domain.Entities;

namespace NextErp.Application.Services;

public class StockService(
    IApplicationDbContext dbContext,
    IBranchProvider branchProvider)
    : IStockService
{
    public Task<bool> CheckStockAvailabilityAsync(
        int productVariantId,
        decimal requiredQuantity,
        CancellationToken cancellationToken = default) =>
        requiredQuantity <= 0
            ? Task.FromResult(true)
            : CheckAvailabilityInternalAsync(productVariantId, requiredQuantity, cancellationToken);

    public Task<decimal> GetAvailableStockAsync(int productVariantId, CancellationToken cancellationToken = default) =>
        GetAvailableQuantityReadOnlyAsync(productVariantId, cancellationToken);

    public async Task SetAvailableQuantityAsync(
        int productVariantId,
        decimal targetQuantity,
        CancellationToken cancellationToken = default)
    {
        if (targetQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(targetQuantity));

        var current = await GetAvailableQuantityReadOnlyAsync(productVariantId, cancellationToken).ConfigureAwait(false);
        var delta = targetQuantity - current;
        if (delta == 0)
            return;

        if (delta > 0)
            await IncreaseStockAsync(productVariantId, delta, cancellationToken).ConfigureAwait(false);
        else
            await ReduceStockAsync(productVariantId, -delta, cancellationToken).ConfigureAwait(false);
    }

    public async Task ReduceStockAsync(int productVariantId, decimal quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return;

        var variant = await RequireVariantAsync(productVariantId, cancellationToken);
        var branchId = branchProvider.GetRequiredBranchId();
        await RecordMovementAsync(
            productVariantId,
            variant.TenantId,
            branchId,
            -quantity,
            StockMovementType.ManualAdjustment,
            Guid.Empty,
            cancellationToken: cancellationToken);
    }

    public async Task IncreaseStockAsync(int productVariantId, decimal quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return;

        var variant = await RequireVariantAsync(productVariantId, cancellationToken);
        var branchId = branchProvider.GetRequiredBranchId();
        await RecordMovementAsync(
            productVariantId,
            variant.TenantId,
            branchId,
            quantity,
            StockMovementType.ManualAdjustment,
            Guid.Empty,
            cancellationToken: cancellationToken);
    }

    public async Task EnsureStockRecordExistsAsync(int productVariantId, CancellationToken cancellationToken = default)
    {
        var variant = await RequireVariantAsync(productVariantId, cancellationToken);
        var branchId = branchProvider.GetRequiredBranchId();
        _ = await UpsertStockRowAsync(productVariantId, variant.TenantId, branchId, cancellationToken);
    }

    public async Task RecordMovementAsync(
        int productVariantId,
        Guid tenantId,
        Guid branchId,
        decimal quantityDelta,
        StockMovementType movementType,
        Guid referenceId,
        string? reason = null,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        if (quantityDelta == 0)
            return;

        var variant = await RequireVariantAsync(productVariantId, cancellationToken);
        var stock = await GetOrCreateStockForMovementAsync(
            productVariantId,
            tenantId,
            branchId,
            variant,
            quantityDelta,
            cancellationToken);

        await ApplyQuantityChangeAndRecordMovementAsync(
            stock,
            variant,
            branchId,
            quantityDelta,
            movementType,
            referenceId,
            reason,
            notes,
            cancellationToken);

        // Stock is loaded tracked from the DbSet; SaveChanges in the orchestrating handler will persist
        // both the Stock row update and the StockMovement insert.
    }

    private async Task ApplyQuantityChangeAndRecordMovementAsync(
        Stock stock,
        ProductVariant variant,
        Guid branchId,
        decimal quantityDelta,
        StockMovementType movementType,
        Guid referenceId,
        string? reason,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var previousQuantity = stock.AvailableQuantity;
        var newQuantity = previousQuantity + quantityDelta;
        if (newQuantity < 0)
            throw InsufficientStock(variant, previousQuantity, -quantityDelta);

        await dbContext.StockMovements.AddAsync(
            new StockMovement
            {
                Id = Guid.NewGuid(),
                StockId = stock.Id,
                ProductVariantId = variant.Id,
                BranchId = branchId,
                IsActive = true,
                QuantityChanged = quantityDelta,
                PreviousQuantity = previousQuantity,
                NewQuantity = newQuantity,
                MovementType = movementType,
                ReferenceId = referenceId,
                Reason = reason,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            },
            cancellationToken);

        stock.AvailableQuantity = newQuantity;
        stock.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<Stock> GetOrCreateStockForMovementAsync(
        int productVariantId,
        Guid tenantId,
        Guid branchId,
        ProductVariant variant,
        decimal quantityDelta,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetTrackedStockByVariantAndBranchAsync(productVariantId, branchId, cancellationToken);

        if (existing != null)
            return existing;

        if (quantityDelta < 0)
            throw InsufficientStock(variant, 0, -quantityDelta);

        var row = CreateStockRow(variant, tenantId, branchId, initialAvailable: 0);
        dbContext.Stocks.Add(row);
        return row;
    }

    private async Task<bool> CheckAvailabilityInternalAsync(
        int productVariantId,
        decimal requiredQuantity,
        CancellationToken cancellationToken = default)
    {
        var available = await GetAvailableQuantityReadOnlyAsync(productVariantId, cancellationToken);
        return available >= requiredQuantity;
    }

    private async Task<decimal> GetAvailableQuantityReadOnlyAsync(
        int productVariantId,
        CancellationToken cancellationToken = default)
    {
        _ = await RequireVariantAsync(productVariantId, cancellationToken);
        var branchId = branchProvider.GetRequiredBranchId();
        var existing = await GetTrackedStockByVariantAndBranchAsync(productVariantId, branchId, cancellationToken);

        return existing?.AvailableQuantity ?? 0m;
    }

    private async Task<ProductVariant> RequireVariantAsync(int productVariantId, CancellationToken cancellationToken = default)
    {
        var variant = await dbContext.ProductVariants
            .FirstOrDefaultAsync(pv => pv.Id == productVariantId, cancellationToken);

        return variant
               ?? throw new InvalidOperationException($"Product variant {productVariantId} was not found.");
    }

    // Inlined from former IStockRepository.GetByProductVariantIdAndBranchIdAsync — returns tracked entity
    // so callers can mutate AvailableQuantity directly and have SaveChanges persist it.
    //
    // Checks the change tracker FIRST (DbSet.Local) before hitting the DB. This avoids a
    // production-breaking race in handlers that stage a Stock row via
    // EnsureStockRecordExistsAsync and then call SetAvailableQuantityAsync /
    // RecordMovementAsync before SaveChanges flushes — without the Local check, the second
    // call queried the DB, missed the staged Added entity, and inserted a duplicate row,
    // tripping the UNIQUE(ProductVariantId, BranchId) index on SaveChanges.
    private Task<Stock?> GetTrackedStockByVariantAndBranchAsync(
        int productVariantId,
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        var local = dbContext.Stocks.Local
            .FirstOrDefault(s => s.ProductVariantId == productVariantId && s.BranchId == branchId);
        if (local != null)
            return Task.FromResult<Stock?>(local);

        return dbContext.Stocks
            .Include(s => s.ProductVariant)
                .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(
                s => s.ProductVariantId == productVariantId && s.BranchId == branchId,
                cancellationToken);
    }

    private async Task<Stock> UpsertStockRowAsync(
        int productVariantId,
        Guid tenantId,
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetTrackedStockByVariantAndBranchAsync(productVariantId, branchId, cancellationToken);

        if (existing != null)
            return existing;

        var variant = await RequireVariantAsync(productVariantId, cancellationToken);
        var row = CreateStockRow(variant, tenantId, branchId, initialAvailable: 0);
        dbContext.Stocks.Add(row);
        return row;
    }

    private static Stock CreateStockRow(ProductVariant variant, Guid tenantId, Guid branchId, decimal initialAvailable) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = $"Stock — {variant.Sku}",
            ProductVariantId = variant.Id,
            AvailableQuantity = initialAvailable,
            IsActive = true,
            TenantId = tenantId,
            BranchId = branchId,
            CreatedAt = DateTime.UtcNow
        };

    private static InvalidOperationException InsufficientStock(ProductVariant variant, decimal available, decimal required) =>
        new($"Insufficient stock for SKU '{variant.Sku}'. Available: {available}, Required: {required}.");

    // ===========================================================================
    // Batch API — loads variants + stocks once, then serves per-item operations
    // from in-memory context. Eliminates N+1 query patterns in multi-item handlers.
    // ===========================================================================

    public async Task<IReadOnlyDictionary<int, ProductVariant>> LoadVariantsAsync(
        IReadOnlyCollection<int> variantIds,
        CancellationToken cancellationToken = default)
    {
        if (variantIds.Count == 0)
            return new Dictionary<int, ProductVariant>();

        var distinctIds = variantIds.Distinct().ToList();
        var variants = await dbContext.ProductVariants
            .Include(v => v.Product)
            .Where(v => distinctIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        if (variants.Count == distinctIds.Count)
            return variants;

        var missing = string.Join(", ", distinctIds.Except(variants.Keys));
        throw new InvalidOperationException($"Product variant(s) not found: {missing}.");
    }

    public async Task<StockContext> LoadStockContextAsync(
        IReadOnlyDictionary<int, ProductVariant> variants,
        Guid branchId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var ids = variants.Keys.ToList();
        var stockRows = await dbContext.Stocks
            .Where(s => s.BranchId == branchId && ids.Contains(s.ProductVariantId))
            .ToListAsync(cancellationToken);

        // Tracked load (no AsNoTracking) so in-place RemainingQuantity decrements persist on SaveChanges.
        var batchRows = await dbContext.StockBatches
            .Where(b => b.BranchId == branchId
                        && ids.Contains(b.ProductVariantId)
                        && b.RemainingQuantity > 0
                        && b.IsActive)
            .OrderBy(b => b.ReceivedAt)
            .ToListAsync(cancellationToken);

        var stocksByVariant = stockRows.ToDictionary(s => s.ProductVariantId);
        var batchesByVariant = batchRows
            .GroupBy(b => b.ProductVariantId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Self-heal: top up any variant whose batch sum is short of Stock.AvailableQuantity
        // (legacy / pre-ledger rows) with a synthetic opening batch at Product.Cost.
        foreach (var (variantId, stock) in stocksByVariant)
        {
            batchesByVariant.TryGetValue(variantId, out var existing);
            var openSum = existing?.Sum(b => b.RemainingQuantity) ?? 0m;
            var shortfall = stock.AvailableQuantity - openSum;
            if (shortfall <= 0) continue;
            var variant = variants[variantId];
            var opening = StageSyntheticOpeningBatch(variantId, branchId, tenantId, shortfall, variant.Product?.Cost ?? 0m);
            if (existing == null)
            {
                batchesByVariant[variantId] = new List<StockBatch> { opening };
            }
            else
            {
                existing.Add(opening);
            }
        }

        return new StockContext(branchId, tenantId, variants, stocksByVariant, batchesByVariant);
    }

    private StockBatch StageSyntheticOpeningBatch(
        int productVariantId,
        Guid branchId,
        Guid tenantId,
        decimal quantity,
        decimal unitCost)
    {
        var batch = new StockBatch
        {
            Id = Guid.NewGuid(),
            ProductVariantId = productVariantId,
            BranchId = branchId,
            TenantId = tenantId,
            ReceivedAt = DateTime.UtcNow,
            OriginalQuantity = quantity,
            RemainingQuantity = quantity,
            UnitCost = unitCost,
            PurchaseItemId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        dbContext.StockBatches.Add(batch);
        return batch;
    }

    public StockMovement RecordMovement(
        StockContext context,
        int productVariantId,
        decimal quantityDelta,
        StockMovementType movementType,
        Guid referenceId,
        string? reason = null,
        string? notes = null)
    {
        var variant = context.GetVariant(productVariantId);
        var stock = context.GetStockOrNull(productVariantId);

        if (stock == null)
        {
            if (quantityDelta < 0)
                throw InsufficientStock(variant, 0, -quantityDelta);

            stock = CreateStockRow(variant, context.TenantId, context.BranchId, initialAvailable: 0);
            dbContext.Stocks.Add(stock);
            context.RegisterStock(stock);
        }

        var previousQuantity = stock.AvailableQuantity;
        var newQuantity = previousQuantity + quantityDelta;
        if (newQuantity < 0)
            throw InsufficientStock(variant, previousQuantity, -quantityDelta);

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            StockId = stock.Id,
            ProductVariantId = variant.Id,
            BranchId = context.BranchId,
            IsActive = true,
            QuantityChanged = quantityDelta,
            PreviousQuantity = previousQuantity,
            NewQuantity = newQuantity,
            MovementType = movementType,
            ReferenceId = referenceId,
            Reason = reason,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.StockMovements.Add(movement);
        stock.AvailableQuantity = newQuantity;
        stock.UpdatedAt = DateTime.UtcNow;

        return movement;
    }

    public bool HasStockAvailable(StockContext context, int productVariantId, decimal requiredQuantity) =>
        requiredQuantity <= 0 || GetAvailable(context, productVariantId) >= requiredQuantity;

    public decimal GetAvailable(StockContext context, int productVariantId) =>
        context.GetStockOrNull(productVariantId)?.AvailableQuantity ?? 0m;

    // ---- Batch ledger ----

    public StockBatch CreateBatch(
        StockContext context,
        int productVariantId,
        decimal quantity,
        decimal unitCost,
        Guid? purchaseItemId)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Batch quantity must be positive.");

        var variant = context.GetVariant(productVariantId);
        var batch = new StockBatch
        {
            Id = Guid.NewGuid(),
            ProductVariantId = variant.Id,
            BranchId = context.BranchId,
            TenantId = context.TenantId,
            ReceivedAt = DateTime.UtcNow,
            OriginalQuantity = quantity,
            RemainingQuantity = quantity,
            UnitCost = unitCost,
            PurchaseItemId = purchaseItemId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        dbContext.StockBatches.Add(batch);
        context.RegisterBatch(batch);
        return batch;
    }

    public IReadOnlyList<BatchConsumption> ConsumeBatches(
        StockContext context,
        int productVariantId,
        decimal quantity,
        InventoryConsumptionOrder order)
    {
        if (order == InventoryConsumptionOrder.Single || quantity <= 0)
            return Array.Empty<BatchConsumption>();

        var batches = context.GetOpenBatches(productVariantId);
        var walkOrder = order == InventoryConsumptionOrder.Lifo
            ? batches.AsEnumerable().Reverse()
            : batches.AsEnumerable();

        var consumptions = new List<BatchConsumption>();
        var remaining = quantity;
        foreach (var batch in walkOrder)
        {
            if (remaining <= 0) break;
            if (batch.RemainingQuantity <= 0) continue;
            var take = Math.Min(batch.RemainingQuantity, remaining);
            batch.RemainingQuantity -= take;
            batch.UpdatedAt = DateTime.UtcNow;
            remaining -= take;
            consumptions.Add(new BatchConsumption(batch.Id, take, batch.UnitCost));
        }

        if (remaining > 0)
        {
            var variant = context.GetVariant(productVariantId);
            throw new InvalidOperationException(
                $"Batch ledger underflow for SKU '{variant.Sku}': could not consume {quantity} units " +
                $"({remaining} short). This typically indicates a Stock.AvailableQuantity / StockBatch " +
                $"invariant break — investigate before proceeding.");
        }

        return consumptions;
    }

    public async Task SyncBatchesOnAdjustmentAsync(
        int productVariantId,
        Guid branchId,
        Guid tenantId,
        decimal delta,
        CancellationToken cancellationToken = default)
    {
        if (delta == 0) return;

        if (delta > 0)
        {
            // No PO to source cost from — adjustment increase records zero-cost stock.
            dbContext.StockBatches.Add(new StockBatch
            {
                Id = Guid.NewGuid(),
                ProductVariantId = productVariantId,
                BranchId = branchId,
                TenantId = tenantId,
                ReceivedAt = DateTime.UtcNow,
                OriginalQuantity = delta,
                RemainingQuantity = delta,
                UnitCost = 0m,
                PurchaseItemId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            });
            return;
        }

        var toConsume = -delta;
        var batches = await dbContext.StockBatches
            .Where(b => b.BranchId == branchId
                        && b.ProductVariantId == productVariantId
                        && b.RemainingQuantity > 0
                        && b.IsActive)
            .OrderBy(b => b.ReceivedAt)
            .ToListAsync(cancellationToken);

        // AsNoTracking reads pre-adjustment from DB — RecordMovementAsync mutated the
        // tracked entity but SaveChanges hasn't run yet, so DB still holds the old value.
        var openSum = batches.Sum(b => b.RemainingQuantity);
        var stockRow = await dbContext.Stocks
            .AsNoTracking()
            .Where(s => s.ProductVariantId == productVariantId && s.BranchId == branchId)
            .Select(s => new { s.AvailableQuantity })
            .FirstOrDefaultAsync(cancellationToken);
        if (stockRow != null)
        {
            var shortfall = stockRow.AvailableQuantity - openSum;
            if (shortfall > 0)
            {
                var variant = await dbContext.ProductVariants
                    .Include(v => v.Product)
                    .AsNoTracking()
                    .FirstAsync(v => v.Id == productVariantId, cancellationToken);
                var opening = StageSyntheticOpeningBatch(productVariantId, branchId, tenantId, shortfall, variant.Product?.Cost ?? 0m);
                batches.Insert(0, opening);
            }
        }

        foreach (var batch in batches)
        {
            if (toConsume <= 0) break;
            var take = Math.Min(batch.RemainingQuantity, toConsume);
            batch.RemainingQuantity -= take;
            batch.UpdatedAt = DateTime.UtcNow;
            toConsume -= take;
        }

        if (toConsume > 0)
        {
            throw new InvalidOperationException(
                $"Batch ledger underflow on adjustment: variant {productVariantId} short by {toConsume} units. " +
                "This indicates a Stock.AvailableQuantity / StockBatch invariant break.");
        }
    }
}
