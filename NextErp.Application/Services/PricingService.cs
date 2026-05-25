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
///  1. Line-level promotions (LineDiscount, BogoSame, BogoCross) stamp
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

        // Phase A: per-line types (LineDiscount, BogoSame). Each promotion is
        // evaluated against each line in isolation.
        foreach (var promo in eligible.Where(p => IsPerLineLevel(p.Type)))
        {
            // Per-line blocking: a non-stackable promotion of a given type
            // claims the line scope and prevents further promotions of the
            // same type from touching the same variant.
            foreach (var line in lines)
            {
                if (lineTypeBlocked.Contains((line.ProductVariantId, promo.Type)))
                    continue;

                var lineDiscount = ApplyLinePromotion(promo, line);
                if (lineDiscount <= 0) continue;

                lineResults.TryGetValue(line.ProductVariantId, out var current);
                lineResults[line.ProductVariantId] = current + lineDiscount;
                linePromotionIds.TryAdd(line.ProductVariantId, promo.Id);
                if (!promo.Stackable)
                    lineTypeBlocked.Add((line.ProductVariantId, promo.Type));
            }
        }

        // Phase B: BogoCross is cart-aware — the BUY-set qty is summed across
        // the whole cart, not per line, before any GET-set lines are eligible
        // for the discount. Distributed cheapest-first.
        foreach (var promo in eligible.Where(p => p.Type == PromotionType.BogoCross))
        {
            ApplyBogoCross(promo, lines, lineResults, linePromotionIds, lineTypeBlocked);
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
            InvoiceDiscount = decimal.Round(invoiceDiscount, 2, MidpointRounding.AwayFromZero),
            InvoicePromotionId = invoicePromoId,
        };
    }

    private static bool IsPerLineLevel(PromotionType t) =>
        t == PromotionType.LineDiscount
        || t == PromotionType.BogoSame;

    private static decimal ApplyLinePromotion(NextErp.Domain.Entities.Promotion promo, PricingLine line)
    {
        switch (promo.Type)
        {
            case PromotionType.LineDiscount:
                if (!LineMatchesScope(promo.Config, line)) return 0m;
                return ComputeFlatOrPercent(promo.Config, line.Quantity * line.UnitPrice);
            case PromotionType.BogoSame:
                if (!BogoSameMatches(promo.Config, line)) return 0m;
                return ComputeBogoDiscount(promo.Config, line);
            default:
                return 0m;
        }
    }

    // BUY-side items are not discounted — they only qualify the cart for
    // distributing discounts across GET-side lines (cheapest-first).
    private static void ApplyBogoCross(
        NextErp.Domain.Entities.Promotion promo,
        IReadOnlyList<PricingLine> lines,
        Dictionary<int, decimal> lineResults,
        Dictionary<int, Guid> linePromotionIds,
        HashSet<(int variantId, PromotionType type)> lineTypeBlocked)
    {
        var cfg = promo.Config;
        var buyN = cfg.BuyQuantity.GetValueOrDefault();
        var getN = cfg.GetQuantity.GetValueOrDefault();
        var pct = cfg.GetDiscountPercent.GetValueOrDefault();
        if (buyN <= 0 || getN <= 0 || pct <= 0) return;

        var totalBuy = lines.Where(l => BogoCrossBuyMatches(cfg, l)).Sum(l => l.Quantity);
        if (totalBuy < buyN) return;

        var sets = Math.Floor(totalBuy / buyN);
        var unitsRemaining = sets * getN;
        if (unitsRemaining <= 0) return;

        var getCandidates = lines
            .Where(l => BogoCrossGetMatches(cfg, l))
            .Where(l => !lineTypeBlocked.Contains((l.ProductVariantId, promo.Type)))
            .OrderBy(l => l.UnitPrice)
            .ToList();

        foreach (var line in getCandidates)
        {
            if (unitsRemaining <= 0) break;
            var take = Math.Min(line.Quantity, unitsRemaining);
            if (take <= 0) continue;
            var discountForLine = take * line.UnitPrice * (pct / 100m);
            lineResults.TryGetValue(line.ProductVariantId, out var current);
            lineResults[line.ProductVariantId] = current + discountForLine;
            linePromotionIds.TryAdd(line.ProductVariantId, promo.Id);
            if (!promo.Stackable)
                lineTypeBlocked.Add((line.ProductVariantId, promo.Type));
            unitsRemaining -= take;
        }
    }

    private static bool BogoCrossBuyMatches(PromotionConfig cfg, PricingLine line)
    {
        if (cfg.BuyProductId.HasValue && cfg.BuyProductId.Value == line.ProductId) return true;
        if (cfg.BuyCategoryId.HasValue && cfg.BuyCategoryId.Value == line.CategoryId) return true;
        return false;
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

    private static bool BogoSameMatches(PromotionConfig cfg, PricingLine line)
    {
        if (cfg.BogoVariantId.HasValue && cfg.BogoVariantId.Value == line.ProductVariantId) return true;
        if (cfg.BogoProductId.HasValue && cfg.BogoProductId.Value == line.ProductId) return true;
        return false;
    }

    private static bool BogoCrossGetMatches(PromotionConfig cfg, PricingLine line)
    {
        if (cfg.GetProductId.HasValue && cfg.GetProductId.Value == line.ProductId) return true;
        if (cfg.GetCategoryId.HasValue && cfg.GetCategoryId.Value == line.CategoryId) return true;
        return false;
    }

    /// <summary>
    /// "Buy X, get Y at Z% off" — count how many free/discounted units the
    /// quantity is eligible for, then discount that subset. Example:
    /// "Buy 2 get 1 free" on qty 6 → 2 sets → 2 free units → 2×price discount.
    /// </summary>
    private static decimal ComputeBogoDiscount(PromotionConfig cfg, PricingLine line)
    {
        var buy = cfg.BuyQuantity.GetValueOrDefault();
        var get = cfg.GetQuantity.GetValueOrDefault();
        var pct = cfg.GetDiscountPercent.GetValueOrDefault();
        if (buy <= 0 || get <= 0 || pct <= 0) return 0m;

        var setSize = buy + get;
        if (line.Quantity < setSize) return 0m;

        var sets = Math.Floor(line.Quantity / setSize);
        var discountedUnits = sets * get;
        var perUnit = line.UnitPrice * (pct / 100m);
        return decimal.Round(discountedUnits * perUnit, 2, MidpointRounding.AwayFromZero);
    }
}
