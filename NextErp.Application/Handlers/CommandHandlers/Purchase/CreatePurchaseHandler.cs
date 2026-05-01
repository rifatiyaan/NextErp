using MediatR;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using NextErp.Application.Services;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Purchase;

public class CreatePurchaseHandler(
    IApplicationDbContext dbContext,
    IStockService stockService,
    IBranchProvider branchProvider)
    : IRequestHandler<CreatePurchaseCommand, Guid>
{
    public async Task<Guid> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
            throw new InvalidOperationException("A purchase must contain at least one line item.");

        // ---- Phase 1: variants in one query ----
        var variantIds = request.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var variants = await stockService.LoadVariantsAsync(variantIds, cancellationToken);

        var tenantId = variants.Values.First().TenantId;
        var branchId = ResolveWriteBranchId(variants.Values);

        // ---- Phase 2: stocks for branch in one query ----
        var stockContext = await stockService.LoadStockContextAsync(variants, branchId, tenantId, cancellationToken);

        // ---- Phase 3: build purchase + line items + stock movements (no DB calls) ----
        var purchase = CreatePurchaseHeader(request, tenantId, branchId);
        dbContext.Purchases.Add(purchase);
        purchase.TotalAmount = AddLineItemsAndMovements(purchase, request, stockContext);

        // ---- Phase 4: single SaveChanges persists everything ----
        await dbContext.SaveChangesAsync(cancellationToken);
        return purchase.Id;
    }

    private static Entities.Purchase CreatePurchaseHeader(
        CreatePurchaseCommand request,
        Guid tenantId,
        Guid branchId)
    {
        var number = string.IsNullOrWhiteSpace(request.PurchaseNumber)
            ? $"PUR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}"
            : request.PurchaseNumber;

        return new Entities.Purchase
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            PurchaseNumber = number,
            PartyId = request.PartyId,
            PurchaseDate = request.PurchaseDate,
            TotalAmount = 0,
            Discount = request.Discount,
            Metadata = new Entities.Purchase.PurchaseMetadata
            {
                BatchNo = request.Metadata?.BatchNo,
                BillNo = request.Metadata?.BillNo,
                ChallanNo = request.Metadata?.ChallanNo,
                ReferenceNo = request.Metadata?.ReferenceNo,
                Notes = request.Metadata?.Notes
            },
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            TenantId = tenantId,
            BranchId = branchId
        };
    }

    private decimal AddLineItemsAndMovements(
        Entities.Purchase purchase,
        CreatePurchaseCommand request,
        StockContext stockContext)
    {
        decimal total = 0;

        foreach (var dto in request.Items)
        {
            var variant = stockContext.GetVariant(dto.ProductVariantId);
            var title = string.IsNullOrWhiteSpace(dto.Title)
                ? $"{variant.Product?.Title ?? "Product"} — {variant.Title}"
                : dto.Title;

            var item = new Entities.PurchaseItem
            {
                Id = Guid.NewGuid(),
                Title = title,
                PurchaseId = purchase.Id,
                ProductVariantId = variant.Id,
                Quantity = dto.Quantity,
                UnitCost = dto.UnitCost,
                Metadata = new Entities.PurchaseItem.PurchaseItemMetadata
                {
                    Description = dto.Metadata?.Description,
                    Weight = dto.Metadata?.Weight,
                    ExpiryDate = dto.Metadata?.ExpiryDate,
                    BatchNumber = dto.Metadata?.BatchNumber
                },
                CreatedAt = DateTime.UtcNow,
                TenantId = purchase.TenantId
            };

            purchase.Items.Add(item);
            total += item.Total;

            // Pure in-memory: mutates tracked Stock + adds StockMovement.
            stockService.RecordMovement(
                stockContext,
                variant.Id,
                dto.Quantity,
                Entities.StockMovementType.Purchase,
                purchase.Id);
        }

        return total;
    }

    private Guid ResolveWriteBranchId(IEnumerable<Entities.ProductVariant> variants)
    {
        if (!branchProvider.IsGlobal())
            return branchProvider.GetRequiredBranchId();

        var claimBranchId = branchProvider.GetBranchId();
        if (claimBranchId.HasValue)
            return claimBranchId.Value;

        var fromVariant = variants.Select(v => v.BranchId).FirstOrDefault(b => b.HasValue);
        if (fromVariant.HasValue)
            return fromVariant.Value;

        throw new InvalidOperationException("Global user must provide a branch context to create a purchase.");
    }
}
