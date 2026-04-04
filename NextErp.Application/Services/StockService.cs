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
        ResolveAvailableQuantityAsync(productVariantId, cancellationToken);

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
            StockMovementType.Adjustment,
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
            StockMovementType.Adjustment,
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
        StockMovementType type,
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

        var projected = stock.AvailableQuantity + quantityDelta;
        if (projected < 0)
            throw InsufficientStock(variant, stock.AvailableQuantity, -quantityDelta);

        await dbContext.StockMovements.AddAsync(new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductVariantId = productVariantId,
            BranchId = branchId,
            IsActive = true,
            Quantity = quantityDelta,
            Type = type,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        ApplyQuantityChange(stock, variant, quantityDelta);
        await stockRepository.EditAsync(stock);
        await SyncProductAggregateStockAsync(variant.ProductId, cancellationToken);
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
        var available = await ResolveAvailableQuantityAsync(productVariantId, cancellationToken);
        return available >= requiredQuantity;
    }

    private async Task<decimal> ResolveAvailableQuantityAsync(int productVariantId, CancellationToken cancellationToken)
    {
        var variant = await RequireVariantAsync(productVariantId, cancellationToken);
        var branchId = branchProvider.GetRequiredBranchId();
        var stock = await UpsertStockRowAsync(productVariantId, variant.TenantId, branchId, cancellationToken);
        return stock.AvailableQuantity;
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

    private static void ApplyQuantityChange(Stock stock, ProductVariant variant, decimal delta)
    {
        stock.AvailableQuantity = Math.Max(0, stock.AvailableQuantity + delta);
        stock.UpdatedAt = DateTime.UtcNow;
        variant.Stock = (int)Math.Floor(stock.AvailableQuantity);
        variant.UpdatedAt = DateTime.UtcNow;
    }

    private async Task SyncProductAggregateStockAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .Include(p => p.ProductVariants)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
            return;

        product.Stock = product.ProductVariants.Sum(pv => pv.Stock);
        product.UpdatedAt = DateTime.UtcNow;
    }

    private static InvalidOperationException InsufficientStock(ProductVariant variant, decimal available, decimal required) =>
        new($"Insufficient stock for SKU '{variant.Sku}'. Available: {available}, Required: {required}.");
}
