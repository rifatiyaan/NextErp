using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.PurchaseReturn;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.PurchaseReturn;

/// <summary>
/// Creates a Purchase Return and decrements stock for every returned line.
/// Mirror of CreateSaleReturnHandler with the stock-delta sign flipped — a
/// purchase return sends goods back to the supplier, so available stock
/// drops. We refuse to return more than was originally received per
/// source line to keep the inventory ledger consistent.
/// </summary>
public sealed class CreatePurchaseReturnHandler(
    IApplicationDbContext db,
    IStockService stockService,
    IBranchProvider branchProvider)
    : IRequestHandler<CreatePurchaseReturnCommand, Guid>
{
    public async Task<Guid> Handle(CreatePurchaseReturnCommand request, CancellationToken cancellationToken = default)
    {
        var input = request.Request;
        if (input.Items.Count == 0)
            throw new InvalidOperationException("A return must contain at least one line.");

        var purchase = await db.Purchases
            .Include(p => p.Items)
            .Include(p => p.Party)
            .FirstOrDefaultAsync(p => p.Id == input.PurchaseId, cancellationToken)
            ?? throw new InvalidOperationException($"Purchase {input.PurchaseId} not found.");

        if (!purchase.IsActive)
            throw new InvalidOperationException("Cannot return against an inactive purchase.");

        var purchaseItemsById = purchase.Items.ToDictionary(i => i.Id);
        foreach (var line in input.Items)
        {
            if (line.Quantity <= 0)
                throw new InvalidOperationException("Return quantity must be greater than zero.");
            if (!purchaseItemsById.TryGetValue(line.PurchaseItemId, out var srcItem))
                throw new InvalidOperationException(
                    $"Purchase item {line.PurchaseItemId} does not belong to purchase {purchase.Id}.");
            if (line.Quantity > srcItem.Quantity)
                throw new InvalidOperationException(
                    $"Return quantity {line.Quantity} exceeds original received quantity {srcItem.Quantity}.");
            if (line.ProductVariantId != srcItem.ProductVariantId)
                throw new InvalidOperationException(
                    "Return line variant must match the original purchase line variant.");
        }

        var variantIds = input.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var variants = await stockService.LoadVariantsAsync(variantIds, cancellationToken);
        var tenantId = purchase.TenantId;
        var branchId = branchProvider.IsGlobal()
            ? purchase.BranchId
            : branchProvider.GetRequiredBranchId();
        var stockContext = await stockService.LoadStockContextAsync(variants, branchId, tenantId, cancellationToken);

        // Belt-and-braces: we already drop a check at handler entry for
        // negative quantity, but we also need to confirm we have stock to
        // give back to the supplier — you can't return more than you have.
        var requiredByVariant = input.Items
            .GroupBy(i => i.ProductVariantId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
        foreach (var (variantId, required) in requiredByVariant)
        {
            if (!stockService.HasStockAvailable(stockContext, variantId, required))
            {
                var available = stockService.GetAvailable(stockContext, variantId);
                var variant = stockContext.GetVariant(variantId);
                throw new InvalidOperationException(
                    $"Cannot return {required} of variant '{variant.Sku}' — only {available} on hand.");
            }
        }

        var now = DateTime.UtcNow;
        var purchaseReturnId = Guid.NewGuid();
        var suffix = Guid.NewGuid().ToString()[..8].ToUpperInvariant();
        var returnNumber = $"RET-P-{now:yyyyMMdd}-{suffix}";

        var purchaseReturn = new Entities.PurchaseReturn
        {
            Id = purchaseReturnId,
            ReturnNumber = returnNumber,
            PurchaseId = purchase.Id,
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
            var src = purchaseItemsById[line.PurchaseItemId];
            var unitPrice = line.UnitPrice ?? src.UnitCost;
            var subtotal = unitPrice * line.Quantity;
            total += subtotal;

            purchaseReturn.Items.Add(new Entities.PurchaseReturnItem
            {
                Id = Guid.NewGuid(),
                PurchaseReturnId = purchaseReturnId,
                PurchaseItemId = src.Id,
                ProductVariantId = line.ProductVariantId,
                Quantity = line.Quantity,
                UnitPrice = unitPrice,
                ConditionNote = line.ConditionNote,
                CreatedAt = now,
                TenantId = tenantId,
            });

            // Negative delta — goods leave inventory.
            stockService.RecordMovement(
                stockContext,
                line.ProductVariantId,
                -line.Quantity,
                Entities.StockMovementType.Return,
                purchaseReturnId,
                reason: input.Reason,
                notes: line.ConditionNote);
        }

        purchaseReturn.TotalAmount = total;
        db.PurchaseReturns.Add(purchaseReturn);
        await db.SaveChangesAsync(cancellationToken);
        return purchaseReturnId;
    }
}
