using MediatR;
using NextErp.Application.Commands;
using NextErp.Application.Common.Settings;
using NextErp.Application.Interfaces;
using NextErp.Application.Services;
using NextErp.Application.Settings;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Sale;

public class CreateSaleHandler(
    IApplicationDbContext dbContext,
    IStockService stockService,
    IBranchProvider branchProvider,
    INotificationService notifications,
    IPricingService pricingService,
    ISettingsProvider settingsProvider)
    : IRequestHandler<CreateSaleCommand, Guid>
{
    public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
            throw new InvalidOperationException("A sale must contain at least one line item.");

        // ---- Phase 1: load cart variants ----
        var variantIds = request.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var loaded = await stockService.LoadVariantsAsync(variantIds, cancellationToken);
        var variants = new Dictionary<int, Entities.ProductVariant>(loaded);

        var tenantId = variants.Values.First().TenantId;
        var branchId = ResolveWriteBranchId(variants.Values);
        var lines = NormalizeSaleLines(request, variants);

        // ---- Phase 2: promotion engine. May emit BOGO bonus items for
        // products that aren't in the cart (cross-product rewards). ----
        var pricingResolution = await ResolvePromotionsAsync(request, lines, cancellationToken);
        var inventorySettings = await settingsProvider.GetAsync<InventorySettings>(cancellationToken);
        var consumptionOrder = inventorySettings.ConsumptionOrder;

        // Pull in bonus GET variants not already in the cart so their stock +
        // batches are decremented alongside the typed lines.
        var bonusVariantIds = pricingResolution.BonusItems
            .Select(b => b.ProductVariantId)
            .Where(id => !variants.ContainsKey(id))
            .Distinct()
            .ToList();
        if (bonusVariantIds.Count > 0)
        {
            var bonusVariants = await stockService.LoadVariantsAsync(bonusVariantIds, cancellationToken);
            foreach (var kv in bonusVariants)
                variants[kv.Key] = kv.Value;
        }

        // ---- Phase 3: stock context over cart + bonus variants ----
        var stockContext = await stockService.LoadStockContextAsync(variants, branchId, tenantId, cancellationToken);
        EnsureStockAvailableForAllLines(stockContext, lines);

        // ---- Phase 4: build sale + stage line items + stock movements (no DB calls) ----
        // Items-gross = sum of (qty × unitPrice − all line discounts).
        var grossTotal = lines.Sum(l =>
            l.UnitPrice * l.Quantity - l.Discount - GetRuleDiscount(pricingResolution, l));
        var sale = CreateSaleEntity(request, tenantId, branchId, grossTotal);
        // Tag invoice-level discount source and link any auto-applied promotion.
        var manualInvoiceDiscount = request.Discount;
        sale.Discount = manualInvoiceDiscount + pricingResolution.InvoiceDiscount;
        sale.InvoicePromotionId = pricingResolution.InvoicePromotionId;
        if (manualInvoiceDiscount > 0 && pricingResolution.InvoiceDiscount > 0)
            sale.DiscountSource = Entities.DiscountSource.Manual; // both — manual wins as label
        else if (manualInvoiceDiscount > 0)
            sale.DiscountSource = Entities.DiscountSource.Manual;
        else if (pricingResolution.InvoiceDiscount > 0)
            sale.DiscountSource = Entities.DiscountSource.Promotion;
        dbContext.Sales.Add(sale);

        AddSaleLinesAndStockMovements(sale, lines, stockContext, pricingResolution, consumptionOrder);
        AddOptionalPayment(request, sale);

        await notifications.RecordAsync(
            type: "SaleCreated",
            title: "Sale recorded",
            message: $"{sale.SaleNumber} — {sale.FinalAmount:0.##}",
            relatedEntityType: "Sale",
            relatedEntityId: sale.Id.ToString(),
            cancellationToken: cancellationToken);

        // ---- Phase 5: single round-trip persists everything ----
        await dbContext.SaveChangesAsync(cancellationToken);
        return sale.Id;
    }

    private static List<(Entities.ProductVariant Variant, decimal Quantity, decimal UnitPrice, decimal Discount)> NormalizeSaleLines(
        CreateSaleCommand request,
        IReadOnlyDictionary<int, Entities.ProductVariant> variants)
    {
        return request.Items.Select(dto =>
        {
            if (dto.Quantity <= 0)
                throw new InvalidOperationException("Sale item quantity must be greater than zero.");

            var variant = variants[dto.ProductVariantId];
            // Per-line discount is optional + capped at the line subtotal so
            // we never end up with a negative line total. A NULL means "no
            // discount" so manual entry of 0 also stays clean.
            var rawDiscount = dto.Discount ?? 0m;
            if (rawDiscount < 0) rawDiscount = 0m;
            var lineGross = dto.Quantity * variant.Price;
            var discount = rawDiscount > lineGross ? lineGross : rawDiscount;
            return (variant, dto.Quantity, variant.Price, discount);
        }).ToList();
    }

    private void EnsureStockAvailableForAllLines(
        StockContext stockContext,
        IReadOnlyList<(Entities.ProductVariant Variant, decimal Quantity, decimal UnitPrice, decimal Discount)> lines)
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

    private async Task<PricingResolution> ResolvePromotionsAsync(
        CreateSaleCommand request,
        IReadOnlyList<(Entities.ProductVariant Variant, decimal Quantity, decimal UnitPrice, decimal Discount)> lines,
        CancellationToken cancellationToken)
    {
        var pricingLines = lines.Select(l => new PricingLine(
            ProductVariantId: l.Variant.Id,
            ProductId: l.Variant.ProductId,
            CategoryId: l.Variant.Product?.CategoryId ?? 0,
            Quantity: l.Quantity,
            UnitPrice: l.UnitPrice,
            ManualDiscount: l.Discount)).ToList();
        return await pricingService.ResolveForSaleAsync(
            pricingLines, request.PartyId, DateTime.UtcNow, cancellationToken);
    }

    private static decimal GetRuleDiscount(
        PricingResolution resolution,
        (Entities.ProductVariant Variant, decimal Quantity, decimal UnitPrice, decimal Discount) line)
        => resolution.LineDiscounts.FirstOrDefault(d => d.ProductVariantId == line.Variant.Id)?.RuleDiscount ?? 0m;

    private void AddSaleLinesAndStockMovements(
        Entities.Sale sale,
        IReadOnlyList<(Entities.ProductVariant Variant, decimal Quantity, decimal UnitPrice, decimal Discount)> lines,
        StockContext stockContext,
        PricingResolution resolution,
        InventoryConsumptionOrder consumptionOrder)
    {
        foreach (var (variant, quantity, unitPrice, manualDiscount) in lines)
        {
            var lineTitle = $"{variant.Product?.Title ?? "Product"} — {variant.Title}";
            var ruleApplied = resolution.LineDiscounts.FirstOrDefault(d => d.ProductVariantId == variant.Id);
            var ruleDiscount = ruleApplied?.RuleDiscount ?? 0m;
            var totalDiscount = manualDiscount + ruleDiscount;
            // Source label: if both manual + rule apply, "Manual" wins so
            // reports treat the operator's stamp as the canonical reason.
            Entities.DiscountSource? source =
                manualDiscount > 0 ? Entities.DiscountSource.Manual
                : ruleDiscount > 0 ? Entities.DiscountSource.Promotion
                : null;

            stockService.RecordMovement(
                stockContext,
                variant.Id,
                -quantity,
                Entities.StockMovementType.Sale,
                sale.Id);

            // Empty in Single mode — UnitCostAtSale stays null in that case.
            var consumptions = stockService.ConsumeBatches(stockContext, variant.Id, quantity, consumptionOrder);

            decimal? unitCostAtSale = null;
            if (consumptions.Count > 0)
            {
                var totalQty = consumptions.Sum(c => c.Quantity);
                if (totalQty > 0)
                {
                    unitCostAtSale = decimal.Round(
                        consumptions.Sum(c => c.Quantity * c.UnitCost) / totalQty,
                        4,
                        MidpointRounding.AwayFromZero);
                }
            }

            sale.Items.Add(new Entities.SaleItem
            {
                Id = Guid.NewGuid(),
                Title = lineTitle,
                SaleId = sale.Id,
                ProductVariantId = variant.Id,
                Quantity = quantity,
                Price = unitPrice,
                Discount = totalDiscount,
                DiscountSource = source,
                PromotionId = ruleApplied?.PromotionId,
                UnitCostAtSale = unitCostAtSale,
                CreatedAt = DateTime.UtcNow,
                TenantId = sale.TenantId
            });
        }

        // Phantom SaleItems for BogoSame bonus units: physical stock leaves the
        // store at 100% off. Decrement stock + walk batches at the chosen order.
        foreach (var bonus in resolution.BonusItems)
        {
            if (bonus.Quantity <= 0) continue;
            if (!stockContext.Variants.TryGetValue(bonus.ProductVariantId, out var variant))
                continue;
            var lineTitle = $"{variant.Product?.Title ?? "Product"} — {variant.Title} (free)";

            stockService.RecordMovement(
                stockContext,
                bonus.ProductVariantId,
                -bonus.Quantity,
                Entities.StockMovementType.Sale,
                sale.Id);

            var consumptions = stockService.ConsumeBatches(
                stockContext, bonus.ProductVariantId, bonus.Quantity, consumptionOrder);
            decimal? unitCostAtSale = null;
            if (consumptions.Count > 0)
            {
                var totalQty = consumptions.Sum(c => c.Quantity);
                if (totalQty > 0)
                {
                    unitCostAtSale = decimal.Round(
                        consumptions.Sum(c => c.Quantity * c.UnitCost) / totalQty,
                        4,
                        MidpointRounding.AwayFromZero);
                }
            }

            var bonusDiscount = decimal.Round(
                bonus.Quantity * bonus.UnitPrice * (bonus.DiscountPercent / 100m),
                2,
                MidpointRounding.AwayFromZero);
            sale.Items.Add(new Entities.SaleItem
            {
                Id = Guid.NewGuid(),
                Title = lineTitle,
                SaleId = sale.Id,
                ProductVariantId = bonus.ProductVariantId,
                Quantity = bonus.Quantity,
                Price = bonus.UnitPrice,
                Discount = bonusDiscount, // DiscountPercent off (100% = free)
                DiscountSource = Entities.DiscountSource.Promotion,
                PromotionId = bonus.PromotionId,
                UnitCostAtSale = unitCostAtSale,
                CreatedAt = DateTime.UtcNow,
                TenantId = sale.TenantId
            });
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
