using MediatR;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using NextErp.Application.Services;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Sale;

public class CreateSaleHandler(
    IApplicationDbContext dbContext,
    IStockService stockService,
    IBranchProvider branchProvider)
    : IRequestHandler<CreateSaleCommand, Guid>
{
    public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
            throw new InvalidOperationException("A sale must contain at least one line item.");

        // ---- Phase 1: load all variants in one query ----
        var variantIds = request.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var variants = await stockService.LoadVariantsAsync(variantIds, cancellationToken);

        var tenantId = variants.Values.First().TenantId;
        var branchId = ResolveWriteBranchId(variants.Values);

        // ---- Phase 2: load all stock rows for the resolved branch in one query ----
        var stockContext = await stockService.LoadStockContextAsync(variants, branchId, tenantId, cancellationToken);

        // ---- Phase 3: validate availability against the loaded context (no DB calls) ----
        var lines = NormalizeSaleLines(request, variants);
        EnsureStockAvailableForAllLines(stockContext, lines);

        // ---- Phase 4: build sale + stage line items + stock movements (no DB calls) ----
        var grossTotal = lines.Sum(l => l.UnitPrice * l.Quantity);
        var sale = CreateSaleEntity(request, tenantId, branchId, grossTotal);
        dbContext.Sales.Add(sale);

        AddSaleLinesAndStockMovements(sale, lines, stockContext);
        AddOptionalPayment(request, sale);

        // ---- Phase 5: single round-trip persists everything ----
        await dbContext.SaveChangesAsync(cancellationToken);
        return sale.Id;
    }

    private static List<(Entities.ProductVariant Variant, decimal Quantity, decimal UnitPrice)> NormalizeSaleLines(
        CreateSaleCommand request,
        IReadOnlyDictionary<int, Entities.ProductVariant> variants)
    {
        return request.Items.Select(dto =>
        {
            if (dto.Quantity <= 0)
                throw new InvalidOperationException("Sale item quantity must be greater than zero.");

            var variant = variants[dto.ProductVariantId];
            return (variant, dto.Quantity, variant.Price);
        }).ToList();
    }

    private void EnsureStockAvailableForAllLines(
        StockContext stockContext,
        IReadOnlyList<(Entities.ProductVariant Variant, decimal Quantity, decimal UnitPrice)> lines)
    {
        // Multiple lines for the same variant must accumulate against the same stock row.
        var requiredByVariant = lines
            .GroupBy(l => l.Variant.Id)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        foreach (var (variantId, required) in requiredByVariant)
        {
            if (stockService.HasStockAvailable(stockContext, variantId, required))
                continue;

            var variant = stockContext.GetVariant(variantId);
            var available = stockService.GetAvailable(stockContext, variantId);
            var productTitle = variant.Product?.Title ?? "Product";
            throw new InvalidOperationException(
                $"Insufficient stock for SKU '{variant.Sku}' ({productTitle}). " +
                $"Available: {available}, Required: {required}.");
        }
    }

    private static Entities.Sale CreateSaleEntity(
        CreateSaleCommand request,
        Guid tenantId,
        Guid branchId,
        decimal grossTotal)
    {
        var suffix = Guid.NewGuid().ToString()[..8].ToUpperInvariant();
        var saleNumber = $"SALE-{DateTime.UtcNow:yyyyMMdd}-{suffix}";

        return Entities.Sale.Create(
            id: Guid.NewGuid(),
            saleNumber: saleNumber,
            title: $"Sale - {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
            tenantId: tenantId,
            branchId: branchId,
            partyId: request.PartyId,
            itemsGrossTotal: grossTotal,
            discountRequested: request.Discount,
            taxRate: Entities.Sale.DefaultTaxRate,
            saleDate: DateTime.UtcNow,
            metadata: new Entities.Sale.SaleMetadata { PaymentMethod = request.PaymentMethod });
    }

    private void AddSaleLinesAndStockMovements(
        Entities.Sale sale,
        IReadOnlyList<(Entities.ProductVariant Variant, decimal Quantity, decimal UnitPrice)> lines,
        StockContext stockContext)
    {
        foreach (var (variant, quantity, unitPrice) in lines)
        {
            var lineTitle = $"{variant.Product?.Title ?? "Product"} — {variant.Title}";
            sale.Items.Add(new Entities.SaleItem
            {
                Id = Guid.NewGuid(),
                Title = lineTitle,
                SaleId = sale.Id,
                ProductVariantId = variant.Id,
                Quantity = quantity,
                Price = unitPrice,
                CreatedAt = DateTime.UtcNow,
                TenantId = sale.TenantId
            });

            // Pure in-memory: mutates tracked Stock + adds StockMovement to dbContext.
            stockService.RecordMovement(
                stockContext,
                variant.Id,
                -quantity,
                Entities.StockMovementType.Sale,
                sale.Id);
        }
    }

    private void AddOptionalPayment(CreateSaleCommand request, Entities.Sale sale)
    {
        if (!ShouldCreatePayment(request, sale.FinalAmount))
            return;

        var method = Enum.TryParse<Entities.PaymentMethodType>(request.PaymentMethod, true, out var parsed)
            ? parsed
            : Entities.PaymentMethodType.Other;

        var amount = Math.Min(request.PaidAmount ?? 0m, sale.FinalAmount);
        dbContext.SalePayments.Add(new Entities.SalePayment
        {
            Id = Guid.NewGuid(),
            Title = $"Payment — {DateTime.UtcNow:yyyy-MM-dd}",
            SaleId = sale.Id,
            Amount = amount,
            PaymentMethod = method,
            PaidAt = DateTime.UtcNow,
            TenantId = sale.TenantId,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static bool ShouldCreatePayment(CreateSaleCommand request, decimal finalAmount) =>
        !string.IsNullOrWhiteSpace(request.PaymentMethod)
        && request.PaidAmount is > 0
        && finalAmount > 0;

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

        throw new InvalidOperationException("Global user must provide a branch context to create a sale.");
    }
}
