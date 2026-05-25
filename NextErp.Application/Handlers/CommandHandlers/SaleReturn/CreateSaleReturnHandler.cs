using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.SaleReturn;
using NextErp.Application.Common.Settings;
using NextErp.Application.Interfaces;
using NextErp.Application.Settings;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.SaleReturn;

/// <summary>
/// Creates a Sale Return and reverses stock for every returned line.
/// Each return item triggers a positive-delta <see cref="Entities.StockMovement"/>
/// with type <see cref="Entities.StockMovementType.Return"/> — the same
/// abstraction CreateSale uses for outbound movements, just with the sign
/// flipped. We refuse to return more than was originally sold per source
/// line, otherwise a partial-refund mistake silently inflates stock.
/// </summary>
public sealed class CreateSaleReturnHandler(
    IApplicationDbContext db,
    IStockService stockService,
    IBranchProvider branchProvider,
    ISettingsProvider settingsProvider)
    : IRequestHandler<CreateSaleReturnCommand, Guid>
{
    public async Task<Guid> Handle(CreateSaleReturnCommand request, CancellationToken cancellationToken = default)
    {
        var input = request.Request;
        if (input.Items.Count == 0)
            throw new InvalidOperationException("A return must contain at least one line.");

        // Load the source sale (with items) to inherit tenant/branch + validate quantities.
        var sale = await db.Sales
            .Include(s => s.Items)
            .Include(s => s.Party)
            .FirstOrDefaultAsync(s => s.Id == input.SaleId, cancellationToken)
            ?? throw new InvalidOperationException($"Sale {input.SaleId} not found.");

        if (!sale.IsActive)
            throw new InvalidOperationException("Cannot return against an inactive sale.");

        // Validate every line is against an item that actually belongs to this sale,
        // and the returned quantity doesn't exceed the original line quantity.
        var saleItemsById = sale.Items.ToDictionary(i => i.Id);
        foreach (var line in input.Items)
        {
            if (line.Quantity <= 0)
                throw new InvalidOperationException("Return quantity must be greater than zero.");
            if (!saleItemsById.TryGetValue(line.SaleItemId, out var srcItem))
                throw new InvalidOperationException(
                    $"Sale item {line.SaleItemId} does not belong to sale {sale.Id}.");
            if (line.Quantity > srcItem.Quantity)
                throw new InvalidOperationException(
                    $"Return quantity {line.Quantity} exceeds original sold quantity {srcItem.Quantity}.");
            if (line.ProductVariantId != srcItem.ProductVariantId)
                throw new InvalidOperationException(
                    "Return line variant must match the original sale line variant.");
        }

        // Load variants in one round-trip + open a StockContext for the same branch
        // CreateSale uses so the movements end up on the right stock rows.
        var variantIds = input.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var variants = await stockService.LoadVariantsAsync(variantIds, cancellationToken);
        var tenantId = sale.TenantId;
        var branchId = branchProvider.IsGlobal()
            ? sale.BranchId
            : branchProvider.GetRequiredBranchId();
        var stockContext = await stockService.LoadStockContextAsync(variants, branchId, tenantId, cancellationToken);

        var inventorySettings = await settingsProvider.GetAsync<InventorySettings>(cancellationToken);
        var batchesActive = inventorySettings.ConsumptionOrder != InventoryConsumptionOrder.Single;

        var now = DateTime.UtcNow;
        var saleReturnId = Guid.NewGuid();
        var suffix = Guid.NewGuid().ToString()[..8].ToUpperInvariant();
        var returnNumber = $"RET-S-{now:yyyyMMdd}-{suffix}";

        var saleReturn = new Entities.SaleReturn
        {
            Id = saleReturnId,
            ReturnNumber = returnNumber,
            SaleId = sale.Id,
            ReturnDate = input.ReturnDate ?? now,
            Reason = input.Reason,
            Notes = input.Notes,
            CreatedAt = now,
            IsActive = true,
            TenantId = tenantId,
            BranchId = branchId,
        };

        decimal total = 0m;
        foreach (var line in input.Items)
        {
            // We prefer the caller-supplied UnitPrice (a manager can choose
            // to refund at a different price than the original sale, e.g.
            // restock fee), and fall back to the original sale line price.
            var src = saleItemsById[line.SaleItemId];
            var unitPrice = line.UnitPrice ?? src.Price;
            var subtotal = unitPrice * line.Quantity;
            total += subtotal;

            saleReturn.Items.Add(new Entities.SaleReturnItem
            {
                Id = Guid.NewGuid(),
                SaleReturnId = saleReturnId,
                SaleItemId = src.Id,
                ProductVariantId = line.ProductVariantId,
                Quantity = line.Quantity,
                UnitPrice = unitPrice,
                ConditionNote = line.ConditionNote,
                CreatedAt = now,
                TenantId = tenantId,
            });

            // Positive delta — goods back on the shelf.
            stockService.RecordMovement(
                stockContext,
                line.ProductVariantId,
                +line.Quantity,
                Entities.StockMovementType.Return,
                saleReturnId,
                reason: input.Reason,
                notes: line.ConditionNote);

            // Re-instate cost basis from the original sale; falls back to
            // Product.Cost when UnitCostAtSale wasn't captured (Single-mode sale).
            if (batchesActive)
            {
                var variant = stockContext.GetVariant(line.ProductVariantId);
                var unitCost = src.UnitCostAtSale ?? variant.Product?.Cost ?? 0m;
                stockService.CreateBatch(
                    stockContext,
                    line.ProductVariantId,
                    line.Quantity,
                    unitCost,
                    purchaseItemId: null);
            }
        }

        saleReturn.TotalRefund = total;
        db.SaleReturns.Add(saleReturn);
        await db.SaveChangesAsync(cancellationToken);
        return saleReturnId;
    }
}
