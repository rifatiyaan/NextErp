using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using SaleDto = NextErp.Application.DTOs.Sale;

namespace NextErp.Application.Handlers.QueryHandlers.Sale;

public class PreviewSalePricingHandler(
    IApplicationDbContext db,
    IPricingService pricingService)
    : IRequestHandler<PreviewSalePricingQuery, SaleDto.Response.Preview.Single>
{
    public async Task<SaleDto.Response.Preview.Single> Handle(
        PreviewSalePricingQuery request,
        CancellationToken cancellationToken = default)
    {
        if (request.Lines.Count == 0)
            return new SaleDto.Response.Preview.Single();

        var variantIds = request.Lines.Select(l => l.ProductVariantId).Distinct().ToList();
        var variants = await db.ProductVariants
            .AsNoTracking()
            .Include(pv => pv.Product)
            .Where(pv => variantIds.Contains(pv.Id))
            .ToDictionaryAsync(pv => pv.Id, cancellationToken);

        // Skip unknown variants (transient UI IDs, deleted products) — don't fail the preview.
        var pricingLines = new List<PricingLine>(request.Lines.Count);
        foreach (var line in request.Lines)
        {
            if (!variants.TryGetValue(line.ProductVariantId, out var variant))
                continue;
            pricingLines.Add(new PricingLine(
                ProductVariantId: variant.Id,
                ProductId: variant.ProductId,
                CategoryId: variant.Product?.CategoryId ?? 0,
                Quantity: line.Quantity,
                UnitPrice: line.UnitPrice,
                ManualDiscount: line.ManualDiscount));
        }

        var resolution = await pricingService.ResolveForSaleAsync(
            pricingLines, request.PartyId, DateTime.UtcNow, cancellationToken);

        var promoIds = resolution.LineDiscounts
            .Where(ld => ld.PromotionId.HasValue)
            .Select(ld => ld.PromotionId!.Value)
            .Concat(resolution.InvoicePromotionId.HasValue
                ? new[] { resolution.InvoicePromotionId.Value }
                : Array.Empty<Guid>())
            .Distinct()
            .ToList();

        var promoNames = promoIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.Promotions
                .AsNoTracking()
                .Where(p => promoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        // Tax is a frontend concern — engine does not touch it.
        var subtotal = pricingLines.Sum(l =>
            l.Quantity * l.UnitPrice
            - (resolution.LineDiscounts.FirstOrDefault(d => d.ProductVariantId == l.ProductVariantId)?.RuleDiscount ?? 0m)
            - l.ManualDiscount);
        var finalAmount = Math.Max(0m, subtotal - resolution.InvoiceDiscount);

        return new SaleDto.Response.Preview.Single
        {
            Subtotal = decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero),
            LineDiscounts = resolution.LineDiscounts.Select(d => new SaleDto.Response.Preview.LineDiscount
            {
                ProductVariantId = d.ProductVariantId,
                Discount = d.RuleDiscount,
                PromotionId = d.PromotionId,
                PromotionName = d.PromotionId.HasValue && promoNames.TryGetValue(d.PromotionId.Value, out var name)
                    ? name
                    : null,
            }).ToList(),
            InvoiceDiscount = resolution.InvoiceDiscount,
            InvoicePromotionId = resolution.InvoicePromotionId,
            InvoicePromotionName = resolution.InvoicePromotionId.HasValue
                && promoNames.TryGetValue(resolution.InvoicePromotionId.Value, out var ipName)
                ? ipName
                : null,
            FinalAmount = decimal.Round(finalAmount, 2, MidpointRounding.AwayFromZero),
        };
    }
}
