# NextErp Store — Integrated Ecommerce Storefront (v1) — Design

**Date:** 2026-07-03
**Status:** Approved by owner (design + architecture), pending implementation plan
**Repos affected:** `NextErp` (backend), `NextErp_React` (frontend)

## Problem

NextErp tenants (store owners) have a full catalog, stock, and sales pipeline in the ERP, but no public sales channel. A store owner should be able to switch on a public ecommerce website whose catalog is curated from the ERP (manually selected categories and products), take cash-on-delivery orders from guests, and process those orders inside the ERP.

## Decisions (locked with owner)

| Decision | Choice |
|---|---|
| V1 scope | Browse + cart + order placement. **COD only** — no online payment (v2). |
| Placement | Same Next.js app, new `app/(store)/` route group with its own design layer. |
| Customer identity | **Guest checkout** (name, phone, address). Phone matches/creates ERP `Party` at confirm. |
| Order intake | `OnlineOrder` lands **Pending** → staff **confirms** in ERP → Sale created via existing `CreateSale` pipeline (stock moves only then). |
| Curation | `IsPublishedOnline` flags on `Product` and `Category`, managed from a new **Settings → Ecommerce** page. |
| Design direction | **"Warehouse Editorial"** (see below), researched from award-winning minimal ecommerce. |

## Design direction — "Warehouse Editorial"

North stars: **Polène** (two-zoom-level grid, layered PDP), **Teenage Engineering** (spec-sheet voice, de-emphasized CTAs), **Muji** (two font weights, accent only in hairlines/labels). Secondary: Aesop (editorial PDP prose), Telepathic Instruments (one signature interaction).

**Typography** (all Google Fonts via `next/font/google`, variable, exposed as CSS vars, wired into Tailwind v4 `@theme inline`):
- Display: **Fraunces** — headlines/category names at editorial scale (`clamp(2.75rem, 8vw, 8.5rem)`, lh 0.98, tracking −0.02em)
- UI/body: **Inter** — weights **400/600 only**, body 16px/1.55, max 65ch; eyebrow/nav/buttons 11–12px uppercase +0.08em
- Data: **JetBrains Mono** (tabular) — every number on the site: prices, SKUs, stock counts, order numbers, totals. This is the ERP signature.

**Color** (print-on-paper; no gradients, no shadows, no rounded cards — 2px radius max):

| Token | Hex | Use |
|---|---|---|
| ground | `#FAF9F6` | page background |
| surface | `#F1EFE9` | cards, input fills, image placeholders |
| ink | `#1C1917` | text, primary buttons |
| ink-soft | `#6B6660` | secondary text (contrast floor 4.5:1) |
| line | `#DAD7CF` | 1px hairlines everywhere |
| accent | `#C24A22` (rust) | ONLY: sale/low-stock tags, hover underline, focus ring, active nav marker |

**Layout:** 12-col / 4-col grid, 24px gutters, 5vw margins, 120–160px section spacing, one rule-breaking element per viewport. Homepage: asymmetric hero (image cols 1–8, Fraunces headline hanging cols 9–12) → mono marquee ribbon (COD/shipping notes; pausable) → 3-up category tiles (name in Fraunces, item count in mono) → per-category horizontal product strips (4.5 visible cards) → one editorial block. PLP: 4-col/2-col, 1px gaps, every 8th product a 2×2 editorial tile; hover = crossfade to second image. PDP: left stacked scrolling gallery; right sticky buy box (Fraunces name, mono price + SKU + availability line — "In stock" or the accent "Only 3 left" when quantity ≤ 5; exact large counts are not exposed publicly, ink "Add to bag" — the one full-strength CTA); below, mono **spec-sheet accordions** (signature module); related strip. Cart = mono line-item **order form**; checkout = single 560px column, numbered sections (01 Contact, 02 Delivery, 03 Payment/COD); confirmation = giant mono order number.

**Motion (complete list — nothing else ships):** ① grid→PDP shared-element image morph (React `<ViewTransition>`, Next 16 `experimental.viewTransition`, ~400ms), ② card hover crossfade + 1.03 scale + accent underline draw, ③ scroll reveals (CSS `animation-timeline: view()` behind `@supports`, IntersectionObserver fallback; opacity + 16px translateY), ④ one slow marquee (pausable, a11y-safe), ⑤ add-to-bag feedback (label swap + bag count tick; no drawer hijack). All paths respect `prefers-reduced-motion`.

**Anti-rules:** no pure white/black, no third color, no font weights beyond 400/600, no urgency theater (timers/fake scarcity/popups/newsletter modals/emoji), no hero carousels, no account wall, no accessibility trade-aways (visible 2px focus ring, labels beside icons, ≥4.5:1 text).

## Backend design

### Data model
- `Product.IsPublishedOnline : bool` (default false), `Category.IsPublishedOnline : bool` (default false) — one EF migration.
- **`OnlineOrder`** (new aggregate): `Id`, `OrderNumber` (string, `W` + 6 digits, sequential per tenant via the `ProductCodeFactory` pattern), `CustomerName`, `Phone`, `Address`, `Note?`, `Status` (`Pending | Confirmed | Cancelled`), `CancelReason?`, `DeliveryFee` (snapshot), `PartyId?` (set at confirm), `SaleId?` (set at confirm), `TenantId`, `BranchId` (= selling branch at creation), `CreatedAt`, `ConfirmedAt?`. Enum crosses the wire as string (global `JsonStringEnumConverter`).
- **`OnlineOrderItem`**: `OnlineOrderId`, `ProductVariantId`, snapshots (`ProductTitle`, `Sku`, `UnitPrice`), `Quantity`, `LineTotal`. Snapshots preserve exactly what the customer agreed to (audit-correct).
- **Ecommerce settings** via existing `[SettingsModule("Ecommerce")]` declarative system: `StoreName`, `Tagline?`, `HeroHeadline`, `HeroImageUrl?`, `MarqueeText`, `CodNote`, `DeliveryFee` (flat), `SellingBranchId`, `StorefrontEnabled` (default false).

### Branch handling for anonymous requests (important)
`Product`/`Stock` are `[BranchScoped]`; the global query filter depends on claims that anonymous requests do not have. Store query handlers therefore use `IgnoreQueryFilters()` + an **explicit** `BranchId == EcommerceSettings.SellingBranchId` predicate (explicit beats provider magic for the public surface). `OnlineOrder` is stamped with that branch.

### Public API (new, `[AllowAnonymous]`, `api/store/*`)
| Endpoint | Returns |
|---|---|
| `GET api/store/config` | store name, hero, marquee, COD note, delivery fee, enabled flag |
| `GET api/store/categories` | published+active categories with published-product counts |
| `GET api/store/products?category=&search=&pageIndex=&pageSize=` | paged; only `IsPublishedOnline && IsActive` products whose category is published; public DTO |
| `GET api/store/products/{id}` | detail + variants + per-variant availability from selling branch |
| `POST api/store/orders` | creates Pending `OnlineOrder`; returns order number |

Public DTOs are dedicated types — never expose cost, tenant/branch internals, or admin fields. Stock is exposed as availability (`inStock`, `quantity` only when ≤ 5 for the "3 left" label). All `api/store/*` endpoints sit behind ASP.NET Core **rate limiting** (fixed-window per IP; tighter on `POST orders`). Checkout form includes a **honeypot field**; a filled honeypot silently drops the request. When `StorefrontEnabled=false`, all store endpoints return 403 and the frontend renders a "store closed" page.

`POST orders` validation (FluentValidation): every item published+active+belongs to a published category, quantity 1–99, phone matches a permissive pattern, name/address lengths. Per-line validation errors. Stock is **not** authoritative here — that happens at confirm.

### Admin API (authorized)
- `GET api/ecommerce/publication` — category tree + products with flags/counts; `PUT api/ecommerce/publication` — bulk flag updates.
- `GET api/onlineorder?status=&pageIndex=` / `GET api/onlineorder/{id}`.
- `POST api/onlineorder/{id}/confirm` — matches/creates `Party` by phone, builds a Sale through the **existing `CreateSale` pipeline** with **snapshot prices as manual prices** (`DiscountSource.Manual` semantics; the promotion engine is deliberately NOT re-run — the quoted total is honored), links `SaleId`, sets `Confirmed`. Insufficient stock surfaces the pipeline's error to the staff UI; order stays Pending.
- `POST api/onlineorder/{id}/cancel` — requires reason.
- New online order triggers existing `NotificationService` (`OnlineOrderPlaced`).

## Frontend design

### Storefront (`NextErp_React/app/(store)/`)
Routes: `/` home · `/shop` PLP · `/shop/[category]` · `/product/[id]` PDP · `/cart` · `/checkout` · `/order/[number]` confirmation.
- Route group has its own layout applying store font variables + a scope class for store tokens; admin styling untouched. Note: route groups do not affect URLs — store owns `/`, admin stays under its existing paths.
- Pages are **server components with ISR** (`revalidate` 120s catalog / 60s product detail) fetching the public API server-side; SEO via `generateMetadata`, Product JSON-LD, `sitemap.ts`, `opengraph-image` from product photos.
- Cart: zustand + `persist` (localStorage), hydration-gated badge, client islands only (`AddToBag`, header count, cart page table, checkout form with react-hook-form + zod).
- Checkout POST → success page shows mono order number; cart cleared.
- Storefront-disabled state renders a designed "closed" page from `config`.

### ERP admin (`app/(dashboard)/`)
- **Settings → Ecommerce** (new page + menu item): (a) store settings form (existing settings API), (b) **Publication manager** — category tree with checkboxes, per-category product checkbox list, search, "publish all in category", published counts; bulk save.
- **Online Orders** page: paged table (mono order no, customer, phone, item count, total, status chip, age), row detail (items with snapshots, address, note), **Confirm** (shows created Sale link on success; shows pipeline error and keeps Pending on failure) and **Cancel** (reason required). Pending count badge in the nav.

## Error handling summary
- Public POST: per-line validation problems (400, sanitized ProblemDetails); rate-limit 429; honeypot → silent 200.
- Confirm: pipeline errors (stock, validation) surface verbatim to staff; order state unchanged on failure.
- Public endpoints never leak internals in errors; all public reads tolerate empty catalog (designed empty states).

## Testing
- **Backend TDD (xUnit, RED first):** store queries return only published+active data (and respect the explicit branch predicate); publication bulk-update handler; order create validation (unpublished item, qty bounds, honeypot handled at controller); order number sequencing (`W000001`, continue-from-max); **confirm flow**: Party matched by phone (existing → reused; new → created), Sale created with snapshot prices, stock moved, `SaleId`/`PartyId`/status set; cancel flow; storefront-disabled 403.
- **Frontend:** `tsc --noEmit` gate (no new errors); manual E2E smoke at the end (browse → cart → order → ERP confirm → Sale visible) via the running app.

## Out of scope (v1)
Online payment, customer accounts, order tracking beyond the confirmation page, promotion display on the storefront (base prices only), multi-branch selling, reviews/wishlist, dark mode, storefront localization (English UI).

## Compliance notes
- Order snapshots + confirm-gated stock movement keep the audit trail clean (order ≠ sale until staff action).
- Guest PII (name/phone/address) enters `OnlineOrder`/`Party` — GDPR-relevant; no marketing use in v1; synthetic data only in tests/examples.
- Public API surface is new: rate-limited, anonymous, minimal DTOs (flag for security review before production exposure).
