using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
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
    private Task<Stock?> GetTrackedStockByVariantAndBranchAsync(
        int productVariantId,
        Guid branchId,
        CancellationToken cancellationToken = default) =>
        dbContext.Stocks
            .Include(s => s.ProductVariant)
                .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(
                s => s.ProductVariantId == productVariantId && s.BranchId == branchId,
                cancellationToken);

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

        var stocksByVariant = stockRows.ToDictionary(s => s.ProductVariantId);
        return new StockContext(branchId, tenantId, variants, stocksByVariant);
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
}
