using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Services;

/// <summary>
/// Reference implementation of <see cref="IPricingService"/>. Loads all
/// candidate promotions in one query, sorts by priority + creation date,
/// then walks each rule type's matching logic against the proposed lines.
///
/// Order matters (and is unit-tested):
///  1. Line-level promotions (LineDiscount, Bogo) stamp
///     per-line discounts.
///  2. Manual line discounts stack on top (in CreateSaleHandler, not here).
///  3. Invoice-level promotions (InvoiceDiscount) apply to subtotal.
///  4. Membership applies on subtotal (stacks if Stackable=true).
///  5. Manual invoice discount stacks last (in handler).
///
/// Stacking semantics: within a stackable group, all matching promotions
/// of the same type apply additively (line-level) or sequentially
/// (invoice-level). A non-stackable promotion blocks others of the same
/// type from touching the same scope.
/// </summary>
public sealed class PricingService(IApplicationDbContext db) : IPricingService
{
    public async Task<PricingResolution> ResolveForSaleAsync(
        IReadOnlyList<PricingLine> lines,
        Guid? partyId,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (lines.Count == 0)
            return new PricingResolution();

        // 1. Pull all candidate promotions in one round-trip. Date gates
        //    are evaluated server-side so we never materialise rows that
        //    can't possibly apply.
        var eligible = await db.Promotions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Where(p => p.StartDate == null || p.StartDate <= asOf)
            .Where(p => p.EndDate == null || p.EndDate >= asOf)
            .OrderByDescending(p => p.Priority)
            .ThenBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        // Membership tier lookup — only fetch the party row if any
        // Membership-typed promotion is even eligible, otherwise skip.
        string? customerTier = null;
        if (partyId.HasValue && eligible.Any(p => p.Type == PromotionType.Membership))
        {
            customerTier = await db.Parties
                .AsNoTracking()
                .Where(p => p.Id == partyId.Value)
                .Select(p => p.MembershipTier)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var lineResults = new Dictionary<int, decimal>();   // variantId -> accumulated rule discount
        var linePromotionIds = new Dictionary<int, Guid>(); // variantId -> first applied promo
        var lineTypeBlocked = new HashSet<(int variantId, PromotionType type)>();
        var bonusItems = new List<BonusItem>();             // BOGO auto-added GET lines

        // Phase A: line-level $ discounts (LineDiscount only).
        foreach (var promo in eligible.Where(p => p.Type == PromotionType.LineDiscount))
        {
            foreach (var line in lines)
            {
                if (lineTypeBlocked.Contains((line.ProductVariantId, promo.Type)))
                    continue;
                if (!LineMatchesScope(promo.Config, line)) continue;
                var lineDiscount = ComputeFlatOrPercent(promo.Config, line.Quantity * line.UnitPrice);
                if (lineDiscount <= 0) continue;

                lineResults.TryGetValue(line.ProductVariantId, out var current);
                lineResults[line.ProductVariantId] = current + lineDiscount;
                linePromotionIds.TryAdd(line.ProductVariantId, promo.Id);
                if (!promo.Stackable)
                    lineTypeBlocked.Add((line.ProductVariantId, promo.Type));
            }
        }

        // Phase B: BOGO — cart-aware. BUY-set qty is summed across the whole
        // cart; each GET product is auto-added as a bonus line. GET products'
        // primary variants are batch-loaded once.
        var bogoPromos = eligible.Where(p => p.Type == PromotionType.Bogo).ToList();
        if (bogoPromos.Count > 0)
        {
            var getProductIds = bogoPromos
                .SelectMany(p => p.Config.GetProductIds ?? Enumerable.Empty<int>())
                .Distinct()
                .ToList();
            var getVariants = await LoadPrimaryVariantsAsync(getProductIds, cancellationToken);
            foreach (var promo in bogoPromos)
                ApplyBogo(promo, lines, getVariants, bonusItems);
        }

        // 3. Subtotal AFTER line discounts (manual line discounts are stacked
        //    later in the handler, so we use rule output only here).
        var subtotal = lines.Sum(l =>
            l.Quantity * l.UnitPrice
            - (lineResults.GetValueOrDefault(l.ProductVariantId, 0m))
            - l.ManualDiscount);

        // 4. Invoice-level promotions (Stackable=true → all apply
        //    sequentially; Stackable=false → first one wins, rest blocked).
        decimal invoiceDiscount = 0m;
        Guid? invoicePromoId = null;
        var invoiceBlocked = false;
        foreach (var promo in eligible.Where(p => p.Type == PromotionType.InvoiceDiscount))
        {
            if (invoiceBlocked) break;
            var d = ApplyInvoicePromotion(promo, subtotal - invoiceDiscount);
            if (d <= 0) continue;
            invoiceDiscount += d;
            invoicePromoId ??= promo.Id;
            if (!promo.Stackable) invoiceBlocked = true;
        }

        // 5. Membership — same Stackable rule.
        if (!string.IsNullOrWhiteSpace(customerTier))
        {
            foreach (var promo in eligible.Where(p => p.Type == PromotionType.Membership))
            {
                if (invoiceBlocked) break;
                if (!string.Equals(promo.Config.MembershipTier, customerTier, StringComparison.OrdinalIgnoreCase))
                    continue;
                var pct = promo.Config.DiscountPercent.GetValueOrDefault();
                if (pct <= 0) continue;
                var afterPrior = subtotal - invoiceDiscount;
                if (afterPrior <= 0) break;
                var d = decimal.Round(afterPrior * (pct / 100m), 2, MidpointRounding.AwayFromZero);
                invoiceDiscount += d;
                invoicePromoId ??= promo.Id;
                if (!promo.Stackable) invoiceBlocked = true;
            }
        }

        return new PricingResolution
        {
            LineDiscounts = lineResults.Select(kv => new LineResolution(
                kv.Key,
                decimal.Round(kv.Value, 2, MidpointRounding.AwayFromZero),
                linePromotionIds.TryGetValue(kv.Key, out var pid) ? pid : null)).ToList(),
            BonusItems = bonusItems,
            InvoiceDiscount = decimal.Round(invoiceDiscount, 2, MidpointRounding.AwayFromZero),
            InvoicePromotionId = invoicePromoId,
        };
    }

    // Cart-aware: BUY-set qty summed across all matching lines decides how
    // many "sets" were earned; each GET product is auto-added as a bonus line
    // (getN units per set, at GetDiscountPercent). Same-product BOGO is just
    // the case where GetProductIds holds the BUY product.
    private static void ApplyBogo(
        NextErp.Domain.Entities.Promotion promo,
        IReadOnlyList<PricingLine> lines,
        IReadOnlyDictionary<int, (int VariantId, decimal Price)> getVariants,
        List<BonusItem> bonusItems)
    {
        var cfg = promo.Config;
        var buyN = cfg.BuyQuantity.GetValueOrDefault();
        var getN = cfg.GetQuantity.GetValueOrDefault();
        var pct = cfg.GetDiscountPercent.GetValueOrDefault();
        if (buyN <= 0 || getN <= 0 || pct <= 0) return;
        if (cfg.GetProductIds == null || cfg.GetProductIds.Count == 0) return;

        var totalBuy = lines.Where(l => BogoBuyMatches(cfg, l)).Sum(l => l.Quantity);
        if (totalBuy < buyN) return;
        var sets = Math.Floor(totalBuy / buyN);
        if (sets <= 0) return;

        var qtyPerProduct = sets * getN;
        // Optional cap: never give away more than MaxRewardQuantity units of
        // each GET product, however large the cart.
        if (cfg.MaxRewardQuantity is { } cap && cap > 0 && qtyPerProduct > cap)
            qtyPerProduct = cap;
        foreach (var productId in cfg.GetProductIds.Distinct())
        {
            if (!getVariants.TryGetValue(productId, out var v)) continue;
            bonusItems.Add(new BonusItem(
                ProductVariantId: v.VariantId,
                Quantity: qtyPerProduct,
                UnitPrice: v.Price,
                DiscountPercent: pct,
                PromotionId: promo.Id));
        }
    }

    private static bool BogoBuyMatches(PromotionConfig cfg, PricingLine line)
    {
        if (cfg.BuyProductIds != null && cfg.BuyProductIds.Contains(line.ProductId)) return true;
        if (cfg.BuyCategoryIds != null && cfg.BuyCategoryIds.Contains(line.CategoryId)) return true;
        return false;
    }

    // Resolve each GET product's primary (lowest-id active) variant + price in
    // one round-trip so bonus lines can be auto-added for products not in cart.
    private async Task<Dictionary<int, (int VariantId, decimal Price)>> LoadPrimaryVariantsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken)
    {
        var map = new Dictionary<int, (int, decimal)>();
        if (productIds.Count == 0) return map;

        var rows = await db.ProductVariants
            .AsNoTracking()
            .Where(v => productIds.Contains(v.ProductId))
            .OrderBy(v => v.Id)
            .Select(v => new { v.ProductId, v.Id, v.Price })
            .ToListAsync(cancellationToken);

        foreach (var r in rows)
            if (!map.ContainsKey(r.ProductId))
                map[r.ProductId] = (r.Id, r.Price);
        return map;
    }

    private static decimal ApplyInvoicePromotion(NextErp.Domain.Entities.Promotion promo, decimal subtotal)
    {
        if (subtotal <= 0) return 0m;
        var min = promo.Config.MinSubtotal.GetValueOrDefault();
        if (min > 0 && subtotal < min) return 0m;
        return ComputeFlatOrPercent(promo.Config, subtotal);
    }

    private static decimal ComputeFlatOrPercent(PromotionConfig cfg, decimal baseAmount)
    {
        if (cfg.DiscountAmount.HasValue && cfg.DiscountAmount.Value > 0)
            return Math.Min(cfg.DiscountAmount.Value, baseAmount);
        if (cfg.DiscountPercent.HasValue && cfg.DiscountPercent.Value > 0)
            return decimal.Round(baseAmount * (cfg.DiscountPercent.Value / 100m), 2, MidpointRounding.AwayFromZero);
        return 0m;
    }

    private static bool LineMatchesScope(PromotionConfig cfg, PricingLine line)
    {
        // Multi-select scopes (canonical path from the bulk-picker UI).
        if (cfg.ScopeProductIds != null && cfg.ScopeProductIds.Contains(line.ProductId)) return true;
        if (cfg.ScopeCategoryIds != null && cfg.ScopeCategoryIds.Contains(line.CategoryId)) return true;
        // Legacy single-value fields (kept so older promotions created
        // before the multi-picker rebuild keep applying).
        if (cfg.ScopeProductVariantId.HasValue && cfg.ScopeProductVariantId.Value == line.ProductVariantId) return true;
        if (cfg.ScopeProductId.HasValue && cfg.ScopeProductId.Value == line.ProductId) return true;
        if (cfg.ScopeCategoryId.HasValue && cfg.ScopeCategoryId.Value == line.CategoryId) return true;
        return false;
    }


}
