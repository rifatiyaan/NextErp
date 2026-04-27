using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
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

        var variants = await LoadVariantsAsync(request, cancellationToken);
        var tenantId = variants.Values.First().TenantId;
        var branchId = ResolveWriteBranchId(variants.Values);

        var purchase = CreatePurchaseHeader(request, tenantId, branchId);
        dbContext.Purchases.Add(purchase);

        purchase.TotalAmount = await AddLineItemsAndMovementsAsync(purchase, request, variants, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return purchase.Id;
    }

    private async Task<Dictionary<int, Entities.ProductVariant>> LoadVariantsAsync(
        CreatePurchaseCommand request,
        CancellationToken cancellationToken = default)
    {
        var variantIds = request.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var variants = await dbContext.ProductVariants
            .Include(v => v.Product)
            .Where(v => variantIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        if (variants.Count == variantIds.Count)
            return variants;

        var missing = string.Join(", ", variantIds.Except(variants.Keys));
        throw new InvalidOperationException($"Product variant(s) not found: {missing}.");
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

    private async Task<decimal> AddLineItemsAndMovementsAsync(
        Entities.Purchase purchase,
        CreatePurchaseCommand request,
        IReadOnlyDictionary<int, Entities.ProductVariant> variants,
        CancellationToken cancellationToken = default)
    {
        decimal total = 0;

        foreach (var dto in request.Items)
        {
            var variant = variants[dto.ProductVariantId];
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

            await stockService.RecordMovementAsync(
                variant.Id,
                purchase.TenantId,
                purchase.BranchId,
                dto.Quantity,
                Entities.StockMovementType.Purchase,
                purchase.Id,
                cancellationToken: cancellationToken);
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
