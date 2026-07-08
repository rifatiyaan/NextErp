# Storefront v2 — "Systems Integrated" — Design

**Date:** 2026-07-05
**Status:** Shipped + owner-revised. The original "airy editorial" direction below is superseded by the **As-built revisions** section at the bottom (density pivot, commercn look, config currency, banner, settings IA). Read that section for current truth.
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

## As-built revisions (2026-07-05, post-owner review)
The v2 plan (V2-1…V2-5) shipped, then the owner steered several changes. Current truth:

- **Density pivot (supersedes "airy editorial").** Owner referenced BD commerce sites (rokomari, twelvebd, lerevecraze, startech) + commercn.com (shadcn commerce registry): "I want my site like this." Kept the Theme B palette/fonts but went **dense** — square (1:1) product cards, 2/3/4-col tight grids (editorial 2×2 tiles removed), **no add-to-cart on card** (click-through), shorter hero, compact catalog header, PDP spec **table** (SpecSheet). commercn look **reimplemented with our own Theme B tokens** (NOT the external registry, to avoid supply-chain + token clash): `QuantityStepper`, buy box (big price, availability pill, COD trust row), `CartTable` (cart-item-01 line items + sticky summary), split-screen COD checkout, polished order-confirmation.
- **Home banner/carousel (config-driven).** `EcommerceSettings.HeroSlidesJson` → `HeroCarousel`; managed by a **banner manager** (`/settings/ecommerce`, image upload via `POST /api/SystemSettings/image`, GET/PUT `ecommerce/hero-slides`). Slide urls sanitized (http(s)//-relative only) to block stored XSS.
- **Search + zoom overlays** render via `createPortal` (the `backdrop-blur` header established a containing block that broke `position:fixed`).
- **Configurable store currency.** `EcommerceSettings.Currency` (StoreCurrency enum → ISO code + locale in config); storefront formats every price via `formatMoney` (server `Price` uses `getStoreCurrency`, client uses `useStoreMoney`/`StoreConfigProvider`). No hardcoded NOK. (Shopper-switchable multi-currency + FX is a future step.)
- **Selling branch is zero-config.** `EnableBranchSelling` flag (off by default); `SellingBranchAsync` auto-targets the default branch otherwise. `SellingBranchId` is a dynamic branch **dropdown** (`[SettingOptions("branches")]`).
- **Settings IA.** `/settings/ecommerce` is now a user-control-style page with tabs — **Catalog & publication** (left category rail → right product panel, "All products" grouped view), **Home banner**, **Store settings** (the ecommerce module, moved out of Settings → Features). App shell bounded to `100dvh` so `<main>` scrolls (panes scroll independently). List data-tables no longer pad empty filler rows.
- **Reviews** (V2-2) shipped: `Review` aggregate + GET/POST endpoints; PDP summary/list/form.
- **Dev data:** DB seeded with ~100 real products (dummyjson) across ~11 categories, images uploaded to the tenant's Cloudinary, category-appropriate variations (shirts→size×color, shoes→shoe-size×color, laptops→storage×ram×color, watches→dial color, fragrances/beauty→volume; furniture/groceries→none), stock + purchase chains. Seed rows marked `Code LIKE 'SEED-%'`.

## Remaining / next candidates
AI feature for the module (org AI-first mandate — none yet); online-order fulfillment flow polish; shopper-switchable multi-currency + FX; further commercn polish. See [[project_nexterp-storefront-v2]] memory.
