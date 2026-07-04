# Storefront v2 — "Systems Integrated" — Design

**Date:** 2026-07-05
**Status:** Approved by owner (direction + Theme B + build sequence)
**Repos:** `NextErp` (backend) · `NextErp_React` (frontend) — a redesign + feature upgrade of the shipped v1 "Warehouse Editorial" storefront (`app/(store)/`). Same COD, guest-checkout, `SellingBranchId` model as v1.

## Decision
"Systems Integrated" is the **premium v2 of the NextErp storefront**, not a separate project. Sophisticated enterprise frontend, ultra-light MVP backend (COD only; no payment gateway, no login/accounts).

## Design system — Theme B (Premium Light Corporate Tech, with a technical accent)
- **Type:** display `Manrope` (500/600), body `Inter` (400/500), data/specs `JetBrains Mono` (tabular). Sentence case.
- **Tokens** (uniquely-named, registered in `app/globals.css` `@theme`; admin `--color-accent` untouched):
  - `--color-ground #FAFAFA` (page) · `--color-surface #FFFFFF` (cards) · `--color-subtle #F5F6F8` (fills)
  - `--color-ink #0F172A` · `--color-ink-soft #64748B` · `--color-line #E7E9EE`
  - `--color-store-accent #2563EB` (indigo — CTAs/hover/focus) · `--color-store-accent-soft #EEF2FF`
  - `--color-store-ok #16A34A` (in stock) · `--color-store-low #B45309` (low stock)
- **Radius:** 10–14px (soft premium); controls ~10px, cards ~14px. **Spacing:** airy (8px base, 24–32 gutters, generous section rhythm).
- Alternate themes A (dense dark ERP) and C (industrial grid) are future token-swap skins — not built.

## Interaction mechanics (net-new v2)
- **Debounced keyboard-nav search:** WAI-ARIA combobox (input `role=combobox` + `aria-activedescendant`; listbox/options); 180ms debounce + AbortController; ArrowUp/Down move active option (`preventDefault` + scrollIntoView), Enter selects, Esc closes; min-length 2; matched-substring highlight; ⌘K trigger.
- **Image zoom:** pointer-aware — hover-lens on `(pointer:fine)` (rAF-throttled background-position map), modal/pinch-zoom on touch; hi-res `srcset` + LQIP; preload hi-res on `mouseenter`; respects `prefers-reduced-motion`.

## Build sequence (plan-of-plans; each = spec → build + Workflow review → push → checkpoint)
- **V2-1 — Theme B design system re-skin.** Foundation: swap tokens/fonts (Warehouse Editorial → Theme B) in `globals.css` + `fonts.ts` + `store-tokens.css`; then restyle every storefront component (header, footer, marquee, product card, atoms, PLP grid/pagination, PDP gallery/buy-box/spec/related, cart, checkout, order, closed) to the premium look — soft radius, indigo accent for CTAs/hover, `store-ok`/`store-low` for stock states, airy spacing, Manrope display.
- **V2-2 — Reviews.** Backend `Review` aggregate (COD = no-login: name + rating 1–5 + text; moderation flag; per-product) + endpoints (`GET/POST api/store/products/{id}/reviews`, rating summary) + migration + tests; PDP rating summary + list + form.
- **V2-3 — Advanced catalog hub.** Backend price-range params on `GET api/store/products`; frontend facet sidebar (category + price dual-slider, URL-synced) + debounced ⌘K keyboard-nav search combobox.
- **V2-4 — PDP interactions.** Pointer-aware image zoom (lens/modal) + hi-res srcset; spec-sheet + similar-products polish.
- **V2-5 — Split-screen COD checkout.** Single-column → split-screen (left minimal delivery form, right persistent order summary; payment hardcoded COD).

## Page blueprints (Theme B)
- **Homepage/Catalog Hub:** header (logo · ⌘K search · cart) → compact hero + COD trust chip → 2-col: facet sidebar (category, price slider, in-stock) + product grid (cards: image, name, mono price, availability) → results toolbar + pagination.
- **PDP:** breadcrumb → 2-col: gallery (zoom + thumbnail rail) + sticky buy box (title, mono price, availability, qty stepper, Add to bag [one full-strength indigo CTA], COD trust row) → collapsible spec-sheet → integrated reviews → horizontal similar-products row.

## Testing
Backend TDD (xUnit) for the Review model + endpoints; frontend `tsc --noEmit` (baseline 60, zero new) + `next build` + manual smoke. Per-plan Workflow review (controller implements; subagents refuse cross-repo writes).

## Out of scope (v2)
Online payment, accounts/login, order tracking beyond success page, multi-branch selling, dark-mode storefront, storefront localization.
