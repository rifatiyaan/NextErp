using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;

namespace NextErp.Application.Services;

public class StockService(
    IStockRepository stockRepository,
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
            cancellationToken);
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
            cancellationToken);
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
            cancellationToken);

        await stockRepository.EditAsync(stock);
        await SyncProductAggregateStockAsync(variant.ProductId, cancellationToken);
    }

    private async Task ApplyQuantityChangeAndRecordMovementAsync(
        Stock stock,
        ProductVariant variant,
        Guid branchId,
        decimal quantityDelta,
        StockMovementType movementType,
        Guid referenceId,
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
    {
        var existing = await stockRepository.GetByProductVariantIdAndBranchIdAsync(
            productVariantId,
            branchId,
            cancellationToken);

        if (existing != null)
            return existing;

        if (quantityDelta < 0)
            throw InsufficientStock(variant, 0, -quantityDelta);

        var row = CreateStockRow(variant, tenantId, branchId, initialAvailable: 0);
        await stockRepository.AddAsync(row);
        return row;
    }

    private async Task<bool> CheckAvailabilityInternalAsync(
        int productVariantId,
        decimal requiredQuantity,
        CancellationToken cancellationToken)
    {
        var available = await GetAvailableQuantityReadOnlyAsync(productVariantId, cancellationToken);
        return available >= requiredQuantity;
    }

    private async Task<decimal> GetAvailableQuantityReadOnlyAsync(
        int productVariantId,
        CancellationToken cancellationToken)
    {
        _ = await RequireVariantAsync(productVariantId, cancellationToken);
        var branchId = branchProvider.GetRequiredBranchId();
        var existing = await stockRepository.GetByProductVariantIdAndBranchIdAsync(
            productVariantId,
            branchId,
            cancellationToken);

        return existing?.AvailableQuantity ?? 0m;
    }

    private async Task<ProductVariant> RequireVariantAsync(int productVariantId, CancellationToken cancellationToken)
    {
        var variant = await dbContext.ProductVariants
            .FirstOrDefaultAsync(pv => pv.Id == productVariantId, cancellationToken);

        return variant
               ?? throw new InvalidOperationException($"Product variant {productVariantId} was not found.");
    }

    private async Task<Stock> UpsertStockRowAsync(
        int productVariantId,
        Guid tenantId,
        Guid branchId,
        CancellationToken cancellationToken)
    {
        var existing = await stockRepository.GetByProductVariantIdAndBranchIdAsync(
            productVariantId,
            branchId,
            cancellationToken);

        if (existing != null)
            return existing;

        var variant = await RequireVariantAsync(productVariantId, cancellationToken);
        var row = CreateStockRow(variant, tenantId, branchId, initialAvailable: 0);
        await stockRepository.AddAsync(row);
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

    private async Task SyncProductAggregateStockAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
            return;

        var variantIds = await dbContext.ProductVariants
            .Where(pv => pv.ProductId == productId)
            .Select(pv => pv.Id)
            .ToListAsync(cancellationToken);

        var total = variantIds.Count == 0
            ? 0m
            : await dbContext.Stocks
                .Where(s => s.BranchId == product.BranchId && variantIds.Contains(s.ProductVariantId))
                .SumAsync(s => s.AvailableQuantity, cancellationToken);

        product.Stock = (int)Math.Floor(total);
        product.UpdatedAt = DateTime.UtcNow;
    }

    private static InvalidOperationException InsufficientStock(ProductVariant variant, decimal available, decimal required) =>
        new($"Insufficient stock for SKU '{variant.Sku}'. Available: {available}, Required: {required}.");
}
