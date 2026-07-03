# Ecommerce Storefront ("Warehouse Editorial") Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the public, anonymous storefront (`app/(store)/`) in the Next.js app so a guest can browse the curated catalog, add to a cart, and place a cash-on-delivery order that lands as a `Pending` `OnlineOrder` in the ERP.

**Architecture:** All storefront pages are React Server Components with ISR that fetch the already-shipped anonymous backend API (`GET/POST api/store/*`) server-side; interactivity (cart, add-to-bag, checkout form, header count) lives in small client islands backed by a `zustand` + `persist` cart store. The route group has its own layout that applies "Warehouse Editorial" design tokens + fonts inside a scope class so the existing admin styling is untouched. The backend (Plan 1) and admin UI (Plan 2) are complete and pushed. This plan is frontend-only **except Task 0** — a small app-shell reconciliation: because `app/(dashboard)/page.tsx` currently owns `/`, Task 0 relocates the authenticated home to `/dashboard` (owner-approved) so the storefront can own `/`, and gates the root-layout locale sync so anonymous visitors are not bounced to `/login`.

**Tech Stack:** Next.js 16.0.7 (App Router, RSC, ISR, Next 16 async `params`/`searchParams`), React 19.2, TypeScript, Tailwind v4 (`@theme inline`), `next/font/google` (Fraunces, Inter, JetBrains Mono), zustand 5 (+ `persist`), react-hook-form 7 + zod 4, sonner (already mounted at root).

## Global Constraints

- **English UI only** (customer-facing copy is English for v1; no storefront localization). Code/identifiers English.
- **Numbers/prices** render with `Intl.NumberFormat` — never hardcode separators. Store display locale is `en-GB`; currency code from store config is out of scope (v1 uses a single currency symbol helper; see Task 3).
- **Design tokens are fixed** (from the approved design spec `docs/superpowers/specs/2026-07-03-ecommerce-storefront-design.md`): ground `#FAF9F6`, surface `#F1EFE9`, ink `#1C1917`, ink-soft `#6B6660`, line `#DAD7CF`, accent (rust) `#C24A22`. No pure white/black, no third color, no gradients/shadows, radius ≤ 2px. Fonts: Fraunces (display), Inter (400/600 only), JetBrains Mono (every number). These are registered as **uniquely-named** Tailwind tokens (`ground`, `surface`, `ink`, `ink-soft`, `line`, **`store-accent`**, `font-display`, `font-mono-data`) in `app/globals.css` — **never** reusing the admin's `--color-accent` — so admin styling is untouched (see Task 2).
- **Anti-rules:** no urgency theater, no carousels, no account wall, no newsletter modals, no emoji. Visible 2px focus ring, labels beside icons, ≥4.5:1 text contrast. Respect `prefers-reduced-motion` on every animation.
- **Backend contract is frozen** (do not change backend files in this plan). Endpoints and exact camelCase JSON shapes are enumerated in Task 1. FluentValidation failures surface as **HTTP 422** (repo convention). Enum values cross the wire as **strings**.
- **No test runner is installed** (no vitest/jest). Per the design spec's Testing section, the frontend gate is **`tsc --noEmit` with zero NEW errors** plus **manual verification** each task. The current baseline is **61 pre-existing TS errors** (unrelated files); every task must keep the count at 61 and introduce zero errors in files it touches. Pure-logic files (cart store, formatters) get a documented manual REPL/console check in lieu of unit tests.
- **API base URL:** server-side fetches use `process.env.NEXT_PUBLIC_API_BASE_URL` (dev: `http://localhost:5039`). This var is already defined in `.env.development`. `NEXT_PUBLIC_SITE_URL` (used by sitemap/robots in Task 11) is not yet defined; it falls back to `http://localhost:3000` for dev and **MUST be set in the production environment** for correct absolute URLs.
- **Commit style:** conventional commits, footer `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>`. Branch: `main` (owner consent, consistent with Plans 1–2). Do NOT push until the final task.

---

## Backend contract (read once — every task depends on this)

All under `[AllowAnonymous] [Route("api/store")] [EnableRateLimiting("store")] [ServiceFilter(StorefrontEnabledFilter)]`. When `StorefrontEnabled=false`, **every** endpoint returns **403**. JSON is camelCase; enums are strings.

| Endpoint | Query / Body | Response |
|---|---|---|
| `GET api/store/config` | — | `{ storefrontEnabled: bool, storeName, tagline, heroHeadline, heroImageUrl, marqueeText, codNote, deliveryFee: number }` |
| `GET api/store/categories` | — | `Array<{ id: number, title, parentId: number\|null, productCount: number, imageUrl: string\|null }>` |
| `GET api/store/products` | `?categoryId=&searchText=&pageIndex=1&pageSize=24` | `{ total: number, data: Array<{ id, title, price: number, imageUrl: string\|null, secondImageUrl: string\|null, inStock: bool, lowStockQuantity: number\|null, hasVariations: bool }> }` |
| `GET api/store/products/{id}` | path `id:int` | `{ id, title, price, description: string\|null, categoryTitle: string\|null, categoryId, images: string[], variants: Array<{ id, sku, title, price, inStock, lowStockQuantity: number\|null }> }` — or **404** |
| `POST api/store/orders` | body `{ customerName, phone, address, note?, items: Array<{ productVariantId: number, quantity: number }>, website? }` | `{ orderNumber: string }` (e.g. `W000001`; honeypot-tripped → `W000000`) |

Query param names are exactly `categoryId`, `searchText`, `pageIndex`, `pageSize` (NOT `category`/`search`). `pageSize` is clamped server-side to 1–60. `website` is the **honeypot** (must be present in the form, visually hidden, empty for humans). Order validation (→422 on failure): `customerName` non-empty ≤200; `phone` non-empty ≤32 matching `^[0-9+\-\s()]{6,}$`; `address` non-empty ≤1000; `note` ≤1000; `items` non-empty; each `quantity` an integer 1–99. `lowStockQuantity` is a **decimal** (`decimal?` server-side; stock is `decimal(18,2)`), non-null **only when 1–5 units remain** (drives the accent "Only N left" label, **floored for display** — see Task 3); exact larger counts are never exposed.

---

## File structure

Everything new lives under `NextErp_React/app/(store)/` unless noted. Route groups do not affect URLs. **`/` is currently owned by `app/(dashboard)/page.tsx`; Task 0 relocates that authenticated home to `/dashboard` so the storefront can own `/`** (owner-approved). Admin module routes (`/sales`, `/inventory`, `/settings`, …) are unchanged. Task 0 also touches `app/(dashboard)/dashboard/page.tsx` (new), `components/layout/currency-locale-sync.tsx`, and `lib/api/client.ts` (anonymous-auth fixes).

```
app/(store)/
  layout.tsx                     # store scope: fonts + tokens + closed-state gate + header/footer
  _lib/
    store-types.ts               # TS types mirroring the backend DTOs (Task 1)
    store-errors.ts              # StoreClosedError + ValidationError, client-safe (Task 1)
    store-api.ts                 # server-only anonymous ISR fetch helpers (Task 1)
    store-client.ts              # client-safe placeOrder POST (Task 1)
    cart-store.ts                # zustand + persist cart (Task 3)
    format.ts                    # currency/number/mono formatters, availability label (Task 3)
  _components/
    Marquee.tsx                  # pausable mono ribbon (Task 4)
    ProductCard.tsx              # hover crossfade card (Task 4)
    atoms.tsx                    # Eyebrow, Price, AvailabilityLine, SectionHeading (Task 4)
    StoreHeader.tsx              # nav + cart-count island (Task 5)
    CartCount.tsx                # client island, hydration-gated badge (Task 5)
    StoreFooter.tsx              # footer (Task 5)
    AddToBag.tsx                 # client island on PDP (Task 8)
    Reveal.tsx                   # scroll-reveal wrapper, reduced-motion safe (Task 11)
  page.tsx                       # home (Task 6)
  shop/page.tsx                  # PLP all products (Task 7)
  shop/[categoryId]/page.tsx     # PLP by category (Task 7)
  product/[id]/page.tsx          # PDP (Task 8)
  cart/page.tsx                  # cart (Task 9)
  cart/_components/CartTable.tsx # client island (Task 9)
  checkout/page.tsx              # checkout shell (Task 10)
  checkout/_components/CheckoutForm.tsx  # client island (Task 10)
  order/[number]/page.tsx        # confirmation (Task 10)
  sitemap.ts                     # (Task 11)  -> app/sitemap.ts (root, not in group)
  opengraph-image + robots       # (Task 11)
app/(store)/store-tokens.css     # store CSS vars + scope class (Task 2)
```

**Route-segment decision (deviation from spec):** the spec wrote `/shop/[category]`, but the backend exposes categories only by numeric `id` + `title` (no slug). To keep links unambiguous and avoid a slug lookup table, the category route is **`/shop/[categoryId]`** (numeric), e.g. `/shop/12`. Category tiles/links use `/shop/${id}`. Displayed category name comes from the categories API.

**Data-fetch decision:** storefront RSCs fetch with the native `fetch(url, { next: { revalidate } })`. This is a **separate** path from the dashboard's client-side `lib/api/client.ts` (which attaches auth) — store fetches are anonymous and cached. **Images** use plain `<img>` (not `next/image`) with a `surface` placeholder fallback, to avoid `images.remotePatterns` configuration churn for ERP-hosted URLs; note this as a possible v2 upgrade.

---

### Task 0: Reconcile the app shell for a public storefront

**Files:**
- Create: `app/(dashboard)/dashboard/page.tsx`
- Delete: `app/(dashboard)/page.tsx`
- Modify: the login-success redirect + any dashboard "home" links (grep in Step 2)
- Modify: `components/layout/currency-locale-sync.tsx` (gate the fetch on auth)
- Modify: `lib/api/client.ts` (`handleUnauthorized` — don't bounce anonymous store routes)

**Why:** `app/(dashboard)/page.tsx` currently resolves to `/` (a `"use client"` redirect to the user's first module). The storefront home (Task 6) also resolves to `/`; two `page.tsx` files at one path is a **fatal `next build` error** (`typescript.ignoreBuildErrors:true` hides it from `tsc`, so it would only surface at Task 11). Owner decision: the storefront owns `/`; the authenticated home moves to `/dashboard`. Separately, the root layout (`app/layout.tsx`) mounts `CurrencyLocaleSync`, which fetches the `[Authorize]` `/api/feature-settings` endpoint **unconditionally**; for an anonymous visitor that returns 401 and `fetchAPI.handleUnauthorized` does `window.location.replace('/login…')`, so **every store page would bounce to login** (and raise a "Session expired" toast). Both must be fixed before any store route is testable. These edits are the ONLY admin-shell changes in this plan.

- [ ] **Step 1: Relocate the authenticated home to `/dashboard`**

Read `app/(dashboard)/page.tsx` first (it is a `"use client"` component that redirects to the first module, e.g. via `useUserMenu()` + `router.replace`). Create `app/(dashboard)/dashboard/page.tsx` with the SAME logic (copy it verbatim), then delete `app/(dashboard)/page.tsx`. Now `/dashboard` serves the authenticated home and `/` is free for the storefront.

- [ ] **Step 2: Point authenticated redirects at `/dashboard`**

Grep for redirects/links that send authenticated users to the root and change the ones that mean "the authenticated home" to `/dashboard`:
Run (PowerShell): `Get-ChildItem -Recurse -Include *.ts,*.tsx app,components,contexts,lib | Select-String -Pattern 'push\("/"\)|replace\("/"\)|redirectTo.*=.*"/"|href="/"'`
At minimum update: the login-success navigation (after a successful sign-in) and any sidebar/brand "home" link in the dashboard chrome. Change `/` → `/dashboard`. There are no store-facing `/` links yet, so every current hit means the admin home. Verify the login flow lands on `/dashboard`.

- [ ] **Step 3: Stop anonymous visitors bouncing to `/login`**

Read `components/layout/system-settings-sync.tsx` — it is the CORRECT pattern: its query is gated `enabled: isAuthenticated` (from `useAuth()`). Apply the SAME gate to `components/layout/currency-locale-sync.tsx` so its feature-settings fetch never runs for anonymous users. If the value flows through `useSetting`/`useFeatureSettingsValues` (a SHARED hook also used by admin screens — do NOT globally disable it), thread an optional `enabled?: boolean` param down to the underlying `useQuery` and pass `enabled: isAuthenticated` ONLY from `CurrencyLocaleSync`, mirroring `SystemSettingsSync`.

Then add defense-in-depth in `lib/api/client.ts` `handleUnauthorized` (read the function first; add this guard at its top, before it clears the token / redirects):
```ts
const STORE_PREFIXES = ["/shop", "/product", "/cart", "/checkout", "/order"]
const path = typeof window !== "undefined" ? window.location.pathname : ""
const onStoreRoute = path === "/" || STORE_PREFIXES.some((p) => path === p || path.startsWith(p + "/"))
if (onStoreRoute) return // anonymous storefront: never bounce; let the caller handle the 401
```

- [ ] **Step 4: Verify**

`npx tsc --noEmit` → still 61. `npm run dev`, sign in → lands on `/dashboard`; the admin app still works. `/` will 404 until Task 6 (expected mid-plan). Confirm that hitting a not-yet-existing store path like `/shop` while logged OUT returns a 404, **not** a redirect to `/login`, and raises no toast.

- [ ] **Step 5: Commit**

```bash
git add "app/(dashboard)/dashboard/page.tsx" components/layout/currency-locale-sync.tsx lib/api/client.ts
git rm "app/(dashboard)/page.tsx"
# plus any files touched in Step 2
git commit -m "refactor(app): move authenticated home to /dashboard, free / for the storefront

Also gate the root-layout locale sync and handleUnauthorized so anonymous
storefront routes are not redirected to /login.

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 1: Store types + server API layer

**Files:**
- Create: `app/(store)/_lib/store-types.ts`
- Create: `app/(store)/_lib/store-errors.ts`
- Create: `app/(store)/_lib/store-api.ts`
- Create: `app/(store)/_lib/store-client.ts`

**Interfaces:**
- Produces (consumed by every later task):
  - Types: `StoreConfig`, `StoreCategory`, `StoreProductRow`, `StorePagedProducts`, `StoreVariant`, `StoreProductDetail`, `PlaceOrderInput`, `PlaceOrderResult`.
  - Functions: `getStoreConfig(): Promise<StoreConfig>`, `getStoreCategories(): Promise<StoreCategory[]>`, `getStoreProducts(params): Promise<StorePagedProducts>`, `getStoreProduct(id: number): Promise<StoreProductDetail | null>`, `placeOrder(input: PlaceOrderInput): Promise<PlaceOrderResult>`, and the error class `StoreClosedError`.

- [ ] **Step 1: Write the types**

`app/(store)/_lib/store-types.ts`:
```ts
export interface StoreConfig {
    storefrontEnabled: boolean
    storeName: string
    tagline: string
    heroHeadline: string
    heroImageUrl: string
    marqueeText: string
    codNote: string
    deliveryFee: number
}

export interface StoreCategory {
    id: number
    title: string
    parentId: number | null
    productCount: number
    imageUrl: string | null
}

export interface StoreProductRow {
    id: number
    title: string
    price: number
    imageUrl: string | null
    secondImageUrl: string | null
    inStock: boolean
    lowStockQuantity: number | null
    hasVariations: boolean
}

export interface StorePagedProducts {
    total: number
    data: StoreProductRow[]
}

export interface StoreVariant {
    id: number
    sku: string
    title: string
    price: number
    inStock: boolean
    lowStockQuantity: number | null
}

export interface StoreProductDetail {
    id: number
    title: string
    price: number
    description: string | null
    categoryTitle: string | null
    categoryId: number
    images: string[]
    variants: StoreVariant[]
}

export interface PlaceOrderItem {
    productVariantId: number
    quantity: number
}

export interface PlaceOrderInput {
    customerName: string
    phone: string
    address: string
    note?: string
    items: PlaceOrderItem[]
    website?: string // honeypot
}

export interface PlaceOrderResult {
    orderNumber: string
}
```

- [ ] **Step 2: Write the server API layer**

`app/(store)/_lib/store-api.ts`:
```ts
import "server-only"
import type {
    PlaceOrderInput,
    PlaceOrderResult,
    StoreCategory,
    StoreConfig,
    StorePagedProducts,
    StoreProductDetail,
} from "./store-types"

const BASE = process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5039"

/** Thrown when the storefront is disabled (backend returns 403). */
export class StoreClosedError extends Error {
    constructor() {
        super("The storefront is currently closed.")
        this.name = "StoreClosedError"
    }
}

async function getJson<T>(path: string, revalidate: number): Promise<T> {
    const res = await fetch(`${BASE}/api/store${path}`, {
        next: { revalidate },
        headers: { Accept: "application/json" },
    })
    if (res.status === 403) throw new StoreClosedError()
    if (!res.ok) throw new Error(`Store API ${path} failed: ${res.status}`)
    return (await res.json()) as T
}

export function getStoreConfig(): Promise<StoreConfig> {
    return getJson<StoreConfig>("/config", 120)
}

export function getStoreCategories(): Promise<StoreCategory[]> {
    return getJson<StoreCategory[]>("/categories", 120)
}

export function getStoreProducts(params: {
    categoryId?: number
    searchText?: string
    pageIndex?: number
    pageSize?: number
}): Promise<StorePagedProducts> {
    const q = new URLSearchParams()
    if (params.categoryId != null) q.set("categoryId", String(params.categoryId))
    if (params.searchText) q.set("searchText", params.searchText)
    q.set("pageIndex", String(params.pageIndex ?? 1))
    q.set("pageSize", String(params.pageSize ?? 24))
    return getJson<StorePagedProducts>(`/products?${q.toString()}`, 120)
}

export async function getStoreProduct(id: number): Promise<StoreProductDetail | null> {
    const res = await fetch(`${BASE}/api/store/products/${id}`, {
        next: { revalidate: 60 },
        headers: { Accept: "application/json" },
    })
    if (res.status === 403) throw new StoreClosedError()
    if (res.status === 404) return null
    if (!res.ok) throw new Error(`Store API /products/${id} failed: ${res.status}`)
    return (await res.json()) as StoreProductDetail
}

/**
 * Guest checkout POST. Runs from a client island (no ISR). Maps backend
 * status codes to friendly errors:
 *  - 422 validation -> throws ValidationError with field/line messages
 *  - 429 rate limit -> throws Error("Too many requests…")
 *  - 403 disabled   -> throws StoreClosedError
 */
export class ValidationError extends Error {
    messages: string[]
    constructor(messages: string[]) {
        super(messages[0] ?? "Please check the form and try again.")
        this.name = "ValidationError"
        this.messages = messages
    }
}

export async function placeOrder(input: PlaceOrderInput): Promise<PlaceOrderResult> {
    const res = await fetch(`${BASE}/api/store/orders`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Accept: "application/json" },
        body: JSON.stringify(input),
    })
    if (res.status === 403) throw new StoreClosedError()
    if (res.status === 429) throw new Error("Too many requests. Please wait a moment and try again.")
    if (res.status === 422) {
        const body = (await res.json().catch(() => null)) as
            | { errors?: Record<string, string[]>; detail?: string }
            | null
        const messages = body?.errors
            ? Object.values(body.errors).flat()
            : [body?.detail ?? "Please check the form and try again."]
        throw new ValidationError(messages)
    }
    if (!res.ok) throw new Error(`Order failed: ${res.status}`)
    return (await res.json()) as PlaceOrderResult
}
```

**Note (module split — do this exactly, or `StoreClosedError` ends up undefined in the client module):** `placeOrder` runs from a client island and must NOT pull in `server-only`, but it references `StoreClosedError`, which the server-only `store-api.ts` also uses. Restructure into THREE files so both error classes live in a client-safe module:

1. `app/(store)/_lib/store-errors.ts` — **no `server-only`**. Move BOTH error classes here (delete their definitions from `store-api.ts`):
```ts
export class StoreClosedError extends Error {
    constructor() {
        super("The storefront is currently closed.")
        this.name = "StoreClosedError"
    }
}

export class ValidationError extends Error {
    messages: string[]
    constructor(messages: string[]) {
        super(messages[0] ?? "Please check the form and try again.")
        this.name = "ValidationError"
        this.messages = messages
    }
}
```
2. `store-api.ts` — keep `import "server-only"` + the ISR read helpers (`getStoreConfig`/`getStoreCategories`/`getStoreProducts`/`getStoreProduct`); `import { StoreClosedError } from "./store-errors"` (remove the local class shown in Step 2 above).
3. `store-client.ts` — **no `server-only`**; holds the `BASE` const + `placeOrder`; `import { StoreClosedError, ValidationError } from "./store-errors"` (remove their local classes).

Anything that imports `StoreClosedError` (e.g. the Task 5 layout) must import it from `./store-errors`, **not** from `store-api`.

- [ ] **Step 3: Verify types compile**

Run: `cd C:\Personal\nexterp\NextErp_React && npx tsc --noEmit 2>&1 | Select-String "store-types|store-errors|store-api|store-client" ; npx tsc --noEmit 2>&1 | Measure-Object -Line`
Expected: zero lines mentioning the new files (in particular, no `Cannot find name 'StoreClosedError'` in `store-client.ts`); total error count still 61.

- [ ] **Step 4: Commit**

```bash
git add "app/(store)/_lib/store-types.ts" "app/(store)/_lib/store-errors.ts" "app/(store)/_lib/store-api.ts" "app/(store)/_lib/store-client.ts"
git commit -m "feat(store): types and server API layer for the storefront

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 2: Design tokens + fonts + store CSS scope

**Files:**
- Create: `app/(store)/store-tokens.css`
- Create: `app/(store)/_lib/fonts.ts`

**Interfaces:**
- Produces: a `.store-scope` class that (a) declares the store CSS custom properties, (b) sets `background`/`color` to store tokens, (c) is the container the layout (Task 5) wraps children in; and `storeFontClass` — a string of `next/font` `.variable` classes to apply on that container. Tailwind utilities `bg-ground`, `text-ink`, `text-ink-soft`, `border-line`, `bg-surface`, `text-store-accent`, `bg-ink`, plus font utilities `font-display`, `font-mono-data`.

- [ ] **Step 1: Fonts via next/font/google**

`app/(store)/_lib/fonts.ts`:
```ts
import { Fraunces, Inter, JetBrains_Mono } from "next/font/google"

export const fraunces = Fraunces({
    subsets: ["latin"],
    variable: "--font-fraunces",
    display: "swap",
    axes: ["opsz"],
})

export const inter = Inter({
    subsets: ["latin"],
    variable: "--font-inter",
    display: "swap",
    weight: ["400", "600"],
})

export const jetbrainsMono = JetBrains_Mono({
    subsets: ["latin"],
    variable: "--font-jetbrains",
    display: "swap",
    weight: ["400", "500"],
})

export const storeFontClass = `${fraunces.variable} ${inter.variable} ${jetbrainsMono.variable}`
```

- [ ] **Step 2: Register store tokens in the Tailwind entry + a scoped stylesheet**

**Critical mechanism:** Tailwind v4 only generates utilities from `@theme` blocks inside the file that has `@import "tailwindcss"` — in this repo that is `app/globals.css`. A bare `@theme` in a JS-imported file (e.g. `store-tokens.css`) emits **nothing**, so `bg-ground`/`text-ink`/`text-store-accent`/… would silently not exist and every store component would render unstyled (and `tsc` would still pass). Therefore register the store tokens **in `app/globals.css`**, using **uniquely-named** tokens so the admin's `--color-accent` is never redefined.

Append to `app/globals.css` (after the existing `@theme` block — this is purely additive; do NOT edit any existing admin token):
```css
/* Storefront tokens — additive, uniquely named so admin theme-zinc is untouched. */
@theme {
    --color-ground: #faf9f6;
    --color-surface: #f1efe9;
    --color-ink: #1c1917;
    --color-ink-soft: #6b6660;
    --color-line: #dad7cf;
    --color-store-accent: #c24a22;
    --font-display: var(--font-fraunces), Georgia, serif;
    --font-mono-data: var(--font-jetbrains), ui-monospace, monospace;
}
```
This yields the utilities used throughout the store: `bg-ground text-ink text-ink-soft border-line bg-surface bg-ink text-store-accent bg-store-accent border-store-accent font-display font-mono-data` (plus opacity variants like `bg-ground/90`, `bg-store-accent/5`, `border-store-accent/40`). A non-`inline` `@theme` block also emits the `--color-*` custom properties to `:root`, so the plain rules below can reference them.

Create `app/(store)/store-tokens.css` — **plain scoped rules only** (no `@theme`; safe to JS-import from the layout). Because the root `<body>` forces `bg-background text-foreground theme-zinc`, `.store-scope` resets background/color within the storefront subtree:
```css
.store-scope {
    background: var(--color-ground);
    color: var(--color-ink);
    font-family: var(--font-inter), system-ui, sans-serif;
    font-weight: 400;
    min-height: 100dvh;
    -webkit-font-smoothing: antialiased;
}
.store-scope ::selection { background: var(--color-store-accent); color: var(--color-ground); }

/* Editorial display headline helper */
.store-display { font-family: var(--font-fraunces), Georgia, serif; font-weight: 400; line-height: 0.98; letter-spacing: -0.02em; }
/* Eyebrow / nav / button micro-label */
.store-eyebrow { font-family: var(--font-inter), system-ui, sans-serif; font-weight: 600; font-size: 0.6875rem; letter-spacing: 0.08em; text-transform: uppercase; }
/* Tabular data (prices, SKUs, counts, order numbers) */
.store-mono { font-family: var(--font-jetbrains), ui-monospace, monospace; font-variant-numeric: tabular-nums; }

/* Hairline focus ring per the anti-rules */
.store-scope :focus-visible { outline: 2px solid var(--color-store-accent); outline-offset: 2px; }

@media (prefers-reduced-motion: reduce) {
    .store-scope *,
    .store-scope *::before,
    .store-scope *::after {
        animation-duration: 0.001ms !important;
        animation-iteration-count: 1 !important;
        transition-duration: 0.001ms !important;
    }
}
```
The `--font-fraunces`/`--font-inter`/`--font-jetbrains` vars are provided by `storeFontClass` applied on the `.store-scope` wrapper (Task 5).

- [ ] **Step 3: Verify**

Run: `cd C:\Personal\nexterp\NextErp_React && npx tsc --noEmit 2>&1 | Measure-Object -Line`
Expected: still 61 (CSS/font files add no TS errors; `fonts.ts` is typed by next/font). Also confirm `app/globals.css` still contains the ORIGINAL admin `--color-accent` definition **unchanged** — you only appended a new `@theme` block. (Utility emission for `bg-ground`/`text-store-accent`/… is verified once components use them: visually in Task 5 and via `next build` in Task 11 — Tailwind emits only utilities that are actually referenced, so there is nothing to grep until store components exist.)

- [ ] **Step 4: Commit**

```bash
git add app/globals.css "app/(store)/store-tokens.css" "app/(store)/_lib/fonts.ts"
git commit -m "feat(store): Warehouse Editorial design tokens and fonts

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 3: Cart store + formatters

**Files:**
- Create: `app/(store)/_lib/cart-store.ts`
- Create: `app/(store)/_lib/format.ts`

**Interfaces:**
- Produces:
  - `useCart()` (zustand hook) with state `{ items: CartLine[] }` and actions `addItem(line, qty)`, `setQty(productVariantId, qty)`, `removeItem(productVariantId)`, `clear()`.
  - Selectors: `useCartCount(): number`, `useCartSubtotal(): number`, `useCartHydrated(): boolean`.
  - `CartLine` type: `{ productVariantId, productId, title, variantTitle, sku, price, imageUrl, quantity }`.
  - `format.ts`: `formatCurrency(n: number): string`, `formatNumber(n: number): string`, `availabilityLabel(inStock: boolean, lowStockQuantity: number | null): { text: string; accent: boolean }`.

- [ ] **Step 1: Formatters**

`app/(store)/_lib/format.ts`:
```ts
// Store display locale is en-GB (v1, English-only storefront). Currency symbol
// is a single configurable constant; multi-currency is out of scope for v1.
const LOCALE = "en-GB"
const CURRENCY = "NOK"

const currency = new Intl.NumberFormat(LOCALE, {
    style: "currency",
    currency: CURRENCY,
    minimumFractionDigits: 2,
})
const number = new Intl.NumberFormat(LOCALE)

export function formatCurrency(n: number): string {
    return currency.format(n)
}

export function formatNumber(n: number): string {
    return number.format(n)
}

/**
 * Public availability copy. Exact large counts are never exposed; the backend
 * only sends lowStockQuantity when 1–5 remain, which drives the accent label.
 */
export function availabilityLabel(
    inStock: boolean,
    lowStockQuantity: number | null,
): { text: string; accent: boolean } {
    if (!inStock) return { text: "Out of stock", accent: false }
    if (lowStockQuantity != null && lowStockQuantity > 0) {
        // Stock is decimal(18,2) server-side; floor to a clean integer for display.
        return { text: `Only ${Math.floor(lowStockQuantity)} left`, accent: true }
    }
    return { text: "In stock", accent: false }
}
```

- [ ] **Step 2: Cart store**

`app/(store)/_lib/cart-store.ts`:
```ts
"use client"

import { create } from "zustand"
import { persist } from "zustand/middleware"

export interface CartLine {
    productVariantId: number
    productId: number
    title: string
    variantTitle: string
    sku: string
    price: number
    imageUrl: string | null
    quantity: number
}

interface CartState {
    items: CartLine[]
    addItem: (line: Omit<CartLine, "quantity">, qty: number) => void
    setQty: (productVariantId: number, qty: number) => void
    removeItem: (productVariantId: number) => void
    clear: () => void
}

const clampQty = (q: number) => Math.max(1, Math.min(99, Math.floor(q)))

export const useCart = create<CartState>()(
    persist(
        (set) => ({
            items: [],
            addItem: (line, qty) =>
                set((state) => {
                    const existing = state.items.find((i) => i.productVariantId === line.productVariantId)
                    if (existing) {
                        return {
                            items: state.items.map((i) =>
                                i.productVariantId === line.productVariantId
                                    ? { ...i, quantity: clampQty(i.quantity + qty) }
                                    : i,
                            ),
                        }
                    }
                    return { items: [...state.items, { ...line, quantity: clampQty(qty) }] }
                }),
            setQty: (productVariantId, qty) =>
                set((state) => ({
                    items: state.items.map((i) =>
                        i.productVariantId === productVariantId ? { ...i, quantity: clampQty(qty) } : i,
                    ),
                })),
            removeItem: (productVariantId) =>
                set((state) => ({
                    items: state.items.filter((i) => i.productVariantId !== productVariantId),
                })),
            clear: () => set({ items: [] }),
        }),
        {
            name: "nexterp-store-cart",
            // Persist only the line items; actions are recreated on load.
            partialize: (state) => ({ items: state.items }),
        },
    ),
)

// --- Hydration gate ---------------------------------------------------------
// zustand/persist hydrates from localStorage AFTER the first client render, so
// SSR markup and the first client render must NOT read persisted state or the
// cart badge will hydration-mismatch. Islands read `useCartHydrated()` and
// render a stable placeholder until true.
import { useEffect, useState } from "react"

export function useCartHydrated(): boolean {
    const [hydrated, setHydrated] = useState(false)
    useEffect(() => {
        // rehydrate() returns void; the store is already hydrated by the time
        // this effect runs on the client.
        setHydrated(true)
    }, [])
    return hydrated
}

export function useCartCount(): number {
    return useCart((s) => s.items.reduce((n, i) => n + i.quantity, 0))
}

export function useCartSubtotal(): number {
    return useCart((s) => s.items.reduce((sum, i) => sum + i.price * i.quantity, 0))
}
```

- [ ] **Step 3: Verify (tsc + manual logic check)**

Run: `cd C:\Personal\nexterp\NextErp_React && npx tsc --noEmit 2>&1 | Measure-Object -Line`
Expected: still 61; zero errors in `cart-store.ts`/`format.ts`.

Manual logic check (no test runner): reason through and confirm in review —
- `addItem` on an empty cart adds one line at `clampQty(qty)`; adding the same `productVariantId` again sums quantities and re-clamps to ≤99.
- `setQty(id, 0)` clamps to 1; `setQty(id, 500)` clamps to 99.
- `availabilityLabel(false, null)` → "Out of stock" (accent false); `(true, 3)` → "Only 3 left" (accent true); `(true, null)` → "In stock".
- `formatCurrency(1500)` yields the en-GB NOK format (e.g. `NOK 1,500.00`).

- [ ] **Step 4: Commit**

```bash
git add "app/(store)/_lib/cart-store.ts" "app/(store)/_lib/format.ts"
git commit -m "feat(store): zustand cart store and display formatters

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 4: Shared storefront UI primitives

**Files:**
- Create: `app/(store)/_components/atoms.tsx`
- Create: `app/(store)/_components/Marquee.tsx`
- Create: `app/(store)/_components/ProductCard.tsx`

**Interfaces:**
- Consumes: `StoreProductRow` (Task 1), `formatCurrency`/`availabilityLabel` (Task 3), tokens (Task 2).
- Produces: `Eyebrow`, `Price`, `AvailabilityLine`, `SectionHeading` (from `atoms.tsx`); `Marquee`; `ProductCard`.

- [ ] **Step 1: Atoms**

`app/(store)/_components/atoms.tsx`:
```tsx
import { formatCurrency } from "../_lib/format"

export function Eyebrow({ children }: { children: React.ReactNode }) {
    return <span className="store-eyebrow text-ink-soft">{children}</span>
}

export function Price({ value, className = "" }: { value: number; className?: string }) {
    return <span className={`store-mono text-ink ${className}`}>{formatCurrency(value)}</span>
}

export function AvailabilityLine({ text, accent }: { text: string; accent: boolean }) {
    return (
        <span className={`store-mono text-xs ${accent ? "text-store-accent" : "text-ink-soft"}`}>{text}</span>
    )
}

export function SectionHeading({
    eyebrow,
    title,
    className = "",
}: {
    eyebrow?: string
    title: string
    className?: string
}) {
    return (
        <div className={className}>
            {eyebrow ? <div className="mb-2"><Eyebrow>{eyebrow}</Eyebrow></div> : null}
            <h2 className="store-display text-[clamp(1.75rem,4vw,3rem)] text-ink">{title}</h2>
        </div>
    )
}
```

- [ ] **Step 2: Marquee (pausable, reduced-motion safe)**

`app/(store)/_components/Marquee.tsx`:
```tsx
"use client"

import { useState } from "react"

/**
 * One slow mono ribbon. Pausable on hover/focus; the reduced-motion rule in
 * store-tokens.css freezes the animation for users who ask for it.
 */
export function Marquee({ text }: { text: string }) {
    const [paused, setPaused] = useState(false)
    if (!text) return null
    const segment = text.split("·").map((s) => s.trim()).filter(Boolean)
    const content = segment.length ? segment : [text]
    return (
        <div
            className="border-y border-line bg-surface overflow-hidden"
            onMouseEnter={() => setPaused(true)}
            onMouseLeave={() => setPaused(false)}
        >
            <div
                className="flex whitespace-nowrap store-mono text-xs text-ink-soft py-2 gap-12"
                style={{
                    animation: "store-marquee 40s linear infinite",
                    animationPlayState: paused ? "paused" : "running",
                    willChange: "transform",
                }}
                aria-label={text}
            >
                {[0, 1].map((dup) => (
                    <div key={dup} className="flex gap-12 shrink-0" aria-hidden={dup === 1}>
                        {content.map((s, i) => (
                            <span key={`${dup}-${i}`}>{s}</span>
                        ))}
                    </div>
                ))}
            </div>
            <style>{`@keyframes store-marquee { from { transform: translateX(0) } to { transform: translateX(-50%) } }`}</style>
        </div>
    )
}
```

- [ ] **Step 3: ProductCard (hover crossfade + accent underline)**

`app/(store)/_components/ProductCard.tsx`:
```tsx
import Link from "next/link"
import type { StoreProductRow } from "../_lib/store-types"
import { availabilityLabel } from "../_lib/format"
import { AvailabilityLine, Price } from "./atoms"

const PLACEHOLDER = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg'/%3E"

export function ProductCard({ product, priority = false }: { product: StoreProductRow; priority?: boolean }) {
    const avail = availabilityLabel(product.inStock, product.lowStockQuantity)
    const primary = product.imageUrl ?? PLACEHOLDER
    const secondary = product.secondImageUrl
    return (
        <Link href={`/product/${product.id}`} className="group block">
            <div className="relative aspect-[4/5] bg-surface overflow-hidden">
                {/* base image */}
                <img
                    src={primary}
                    alt={product.title}
                    loading={priority ? "eager" : "lazy"}
                    className="absolute inset-0 h-full w-full object-cover transition-opacity duration-500 group-hover:opacity-0"
                    style={{ viewTransitionName: `product-image-${product.id}` }}
                />
                {/* crossfade image on hover; falls back to a subtle scale when absent */}
                {secondary ? (
                    <img
                        src={secondary}
                        alt=""
                        aria-hidden
                        loading="lazy"
                        className="absolute inset-0 h-full w-full object-cover opacity-0 transition-all duration-500 group-hover:opacity-100 group-hover:scale-[1.03]"
                    />
                ) : (
                    <div className="absolute inset-0 transition-transform duration-500 group-hover:scale-[1.03]" />
                )}
                {avail.accent ? (
                    <span className="absolute left-2 top-2 store-eyebrow bg-ground/90 px-1.5 py-0.5 text-store-accent">
                        {avail.text}
                    </span>
                ) : null}
            </div>
            <div className="mt-3 flex items-baseline justify-between gap-3">
                <h3 className="text-sm text-ink">
                    <span className="bg-[linear-gradient(currentColor,currentColor)] bg-[length:0%_1px] bg-left-bottom bg-no-repeat transition-[background-size] duration-300 group-hover:bg-[length:100%_1px] text-store-accent">
                        {product.title}
                    </span>
                </h3>
                <Price value={product.price} className="text-sm shrink-0" />
            </div>
            <div className="mt-1">
                <AvailabilityLine text={avail.text} accent={avail.accent} />
            </div>
        </Link>
    )
}
```
(The `viewTransitionName` inline style is harmless until Task 11 turns on view transitions; leaving it now avoids re-editing the card later. If `tsc` complains that `viewTransitionName` is not a valid CSS property on `CSSProperties`, cast the style object `as React.CSSProperties` — React 19's types include it, but confirm.)

- [ ] **Step 4: Verify + visual smoke**

Run: `cd C:\Personal\nexterp\NextErp_React && npx tsc --noEmit 2>&1 | Measure-Object -Line` → 61.
Visual smoke deferred to Task 5 (needs the layout to render). Confirm no new TS errors in the three files.

- [ ] **Step 5: Commit**

```bash
git add "app/(store)/_components/atoms.tsx" "app/(store)/_components/Marquee.tsx" "app/(store)/_components/ProductCard.tsx"
git commit -m "feat(store): shared UI primitives — atoms, marquee, product card

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 5: Store layout, header, footer, closed-state gate

**Files:**
- Create: `app/(store)/layout.tsx`
- Create: `app/(store)/_components/StoreHeader.tsx`
- Create: `app/(store)/_components/CartCount.tsx`
- Create: `app/(store)/_components/StoreFooter.tsx`

**Interfaces:**
- Consumes: `getStoreConfig`, `StoreClosedError` (Task 1); `storeFontClass`, `store-tokens.css` (Task 2); `useCartCount`, `useCartHydrated` (Task 3); `getStoreCategories` for the header nav (Task 1).
- Produces: the store chrome. Every store page renders inside this layout. When the store is disabled or config fails, the layout renders a designed "closed" page and does not render children.

- [ ] **Step 1: Cart count island**

`app/(store)/_components/CartCount.tsx`:
```tsx
"use client"

import Link from "next/link"
import { useCartCount, useCartHydrated } from "../_lib/cart-store"

export function CartCount() {
    const hydrated = useCartHydrated()
    const count = useCartCount()
    return (
        <Link href="/cart" className="store-eyebrow text-ink hover:text-store-accent inline-flex items-center gap-1.5">
            Bag
            <span className="store-mono text-xs text-ink-soft tabular-nums">
                ({hydrated ? count : 0})
            </span>
        </Link>
    )
}
```

- [ ] **Step 2: Header**

`app/(store)/_components/StoreHeader.tsx`:
```tsx
import Link from "next/link"
import type { StoreCategory } from "../_lib/store-types"
import { CartCount } from "./CartCount"

export function StoreHeader({ storeName, categories }: { storeName: string; categories: StoreCategory[] }) {
    const top = categories.slice(0, 4)
    return (
        <header className="sticky top-0 z-40 border-b border-line bg-ground/90 backdrop-blur">
            <div className="mx-auto flex max-w-[1600px] items-center justify-between px-[5vw] py-4">
                <Link href="/" className="store-display text-lg text-ink">
                    {storeName}
                </Link>
                <nav className="flex items-center gap-6">
                    <Link href="/shop" className="store-eyebrow text-ink hover:text-store-accent">
                        Shop
                    </Link>
                    {top.map((c) => (
                        <Link
                            key={c.id}
                            href={`/shop/${c.id}`}
                            className="store-eyebrow text-ink-soft hover:text-store-accent hidden md:inline"
                        >
                            {c.title}
                        </Link>
                    ))}
                    <CartCount />
                </nav>
            </div>
        </header>
    )
}
```

- [ ] **Step 3: Footer**

`app/(store)/_components/StoreFooter.tsx`:
```tsx
export function StoreFooter({ storeName, codNote }: { storeName: string; codNote: string }) {
    return (
        <footer className="mt-[120px] border-t border-line bg-surface">
            <div className="mx-auto max-w-[1600px] px-[5vw] py-12 flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
                <div>
                    <div className="store-display text-2xl text-ink">{storeName}</div>
                    {codNote ? <p className="store-mono mt-2 text-xs text-ink-soft max-w-[40ch]">{codNote}</p> : null}
                </div>
                <p className="store-mono text-[11px] text-ink-soft">Cash on delivery · Powered by NextErp</p>
            </div>
        </footer>
    )
}
```

- [ ] **Step 4: Layout with closed-state gate**

`app/(store)/layout.tsx`:
```tsx
import "../(store)/store-tokens.css"
import { storeFontClass } from "./_lib/fonts"
import { getStoreCategories, getStoreConfig } from "./_lib/store-api"
import { StoreClosedError } from "./_lib/store-errors"
import { StoreHeader } from "./_components/StoreHeader"
import { StoreFooter } from "./_components/StoreFooter"
import { Marquee } from "./_components/Marquee"

// Designed "closed" page — shown when StorefrontEnabled=false (403) or config
// is unreachable. No admin chrome; store scope only.
function StoreClosed() {
    return (
        <div className={`store-scope ${storeFontClass} flex min-h-[100dvh] items-center justify-center px-[5vw]`}>
            <div className="text-center">
                <div className="store-eyebrow text-ink-soft">NextErp Store</div>
                <h1 className="store-display mt-4 text-[clamp(2rem,6vw,4rem)] text-ink">
                    We&apos;re currently closed
                </h1>
                <p className="store-mono mt-4 text-sm text-ink-soft">Please check back soon.</p>
            </div>
        </div>
    )
}

export default async function StoreLayout({ children }: { children: React.ReactNode }) {
    let config
    let categories
    try {
        config = await getStoreConfig()
        if (!config.storefrontEnabled) return <StoreClosed />
        categories = await getStoreCategories()
    } catch (err) {
        if (err instanceof StoreClosedError) return <StoreClosed />
        throw err
    }

    return (
        <div className={`store-scope ${storeFontClass}`}>
            <StoreHeader storeName={config.storeName} categories={categories} />
            <Marquee text={config.marqueeText} />
            <main className="mx-auto max-w-[1600px] px-[5vw]">{children}</main>
            <StoreFooter storeName={config.storeName} codNote={config.codNote} />
        </div>
    )
}
```

**Known checkpoints (verify, don't assume):**
1. The anonymous-auth fixes were done in **Task 0** (dashboard relocated off `/`; `CurrencyLocaleSync` gated on `isAuthenticated`; `handleUnauthorized` skips redirect on store routes). Re-confirm here: load a store route while logged OUT → **no** `/login` bounce and **no** "Session expired" toast. (Note: `contexts/auth-context.tsx`'s init effect already returns early when there is no token and does not redirect on mount — the real bounce came from the ungated locale-sync fetch, fixed in Task 0, not from `AuthProvider`.)
2. The root `<body>` sets `bg-background text-foreground theme-zinc`. Confirm the `.store-scope` background/color override actually wins visually (it wraps all store content). If admin theme bleeds through, raise specificity or wrap earlier.

- [ ] **Step 5: Verify + visual smoke**

Start the API (must be running with `StorefrontEnabled=true` and a `SellingBranchId` set, and at least one published category+product — seed via the admin `/settings/ecommerce` from Plan 2) and `npm run dev`. Visit `/`:
- Expected (enabled): store-scoped page — header with store name + up to 4 category links + Bag (0), marquee ribbon, empty `<main>` (home comes in Task 6), footer. Ground background, Fraunces logo, mono Bag count.
- Expected (disabled): flip `StorefrontEnabled=false` in `/settings/features` → reload `/` → the "We're currently closed" page.
Run: `npx tsc --noEmit 2>&1 | Measure-Object -Line` → 61.
If the API cannot run in this environment, record that the visual smoke is deferred to Task 11's E2E and the tsc gate stands.

- [ ] **Step 6: Commit**

```bash
git add "app/(store)/layout.tsx" "app/(store)/_components/StoreHeader.tsx" "app/(store)/_components/CartCount.tsx" "app/(store)/_components/StoreFooter.tsx"
git commit -m "feat(store): layout, header, footer and closed-state gate

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 6: Home page

**Files:**
- Create: `app/(store)/page.tsx`

**Interfaces:**
- Consumes: `getStoreConfig`, `getStoreCategories`, `getStoreProducts` (Task 1); `ProductCard`, `SectionHeading`, `Eyebrow` (Task 4); `format` (Task 3).
- Produces: the home route `/`. Server component with ISR (inherits the 120s revalidate from the fetch helpers).

- [ ] **Step 1: Home page**

`app/(store)/page.tsx`:
```tsx
import Link from "next/link"
import { getStoreCategories, getStoreConfig, getStoreProducts } from "./_lib/store-api"
import { ProductCard } from "./_components/ProductCard"
import { Eyebrow, SectionHeading } from "./_components/atoms"

export default async function StoreHome() {
    const [config, categories] = await Promise.all([getStoreConfig(), getStoreCategories()])
    const topCategories = categories.slice(0, 3)

    // One horizontal product strip per top category (first 8 each), fetched in
    // parallel. ISR caches the whole page for 120s.
    const strips = await Promise.all(
        topCategories.map(async (c) => ({
            category: c,
            products: (await getStoreProducts({ categoryId: c.id, pageSize: 8 })).data,
        })),
    )

    const heroImage = config.heroImageUrl || null

    return (
        <div className="pb-24">
            {/* Asymmetric hero: image cols 1–8, headline hanging cols 9–12 */}
            <section className="grid grid-cols-1 gap-6 py-12 md:grid-cols-12 md:py-20">
                <div className="md:col-span-8">
                    <div className="aspect-[16/10] w-full bg-surface overflow-hidden">
                        {heroImage ? (
                            <img src={heroImage} alt="" className="h-full w-full object-cover" />
                        ) : null}
                    </div>
                </div>
                <div className="flex flex-col justify-end md:col-span-4">
                    {config.tagline ? <Eyebrow>{config.tagline}</Eyebrow> : null}
                    <h1 className="store-display mt-3 text-[clamp(2.75rem,8vw,8.5rem)] text-ink">
                        {config.heroHeadline || config.storeName}
                    </h1>
                    <Link href="/shop" className="store-eyebrow mt-6 inline-block text-store-accent">
                        Shop everything →
                    </Link>
                </div>
            </section>

            {/* 3-up category tiles */}
            {topCategories.length > 0 ? (
                <section className="border-t border-line py-16">
                    <SectionHeading eyebrow="Browse" title="Categories" className="mb-8" />
                    <div className="grid grid-cols-1 gap-px bg-line md:grid-cols-3">
                        {topCategories.map((c) => (
                            <Link key={c.id} href={`/shop/${c.id}`} className="group bg-ground p-6">
                                <div className="aspect-[4/3] w-full bg-surface overflow-hidden">
                                    {c.imageUrl ? (
                                        <img
                                            src={c.imageUrl}
                                            alt=""
                                            className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-[1.03]"
                                        />
                                    ) : null}
                                </div>
                                <div className="mt-4 flex items-baseline justify-between">
                                    <span className="store-display text-2xl text-ink group-hover:text-store-accent">
                                        {c.title}
                                    </span>
                                    <span className="store-mono text-xs text-ink-soft">{c.productCount} items</span>
                                </div>
                            </Link>
                        ))}
                    </div>
                </section>
            ) : null}

            {/* Per-category horizontal product strips */}
            {strips.map(({ category, products }) =>
                products.length === 0 ? null : (
                    <section key={category.id} className="border-t border-line py-16">
                        <div className="mb-6 flex items-end justify-between">
                            <SectionHeading eyebrow="Featured" title={category.title} />
                            <Link href={`/shop/${category.id}`} className="store-eyebrow text-store-accent">
                                View all →
                            </Link>
                        </div>
                        <div className="flex gap-4 overflow-x-auto pb-2 [scrollbar-width:thin]">
                            {products.map((p) => (
                                <div key={p.id} className="w-[clamp(180px,22vw,260px)] shrink-0">
                                    <ProductCard product={p} />
                                </div>
                            ))}
                        </div>
                    </section>
                ),
            )}

            {/* One editorial block */}
            {config.codNote ? (
                <section className="border-t border-line py-20">
                    <p className="store-display mx-auto max-w-[20ch] text-center text-[clamp(1.5rem,3vw,2.5rem)] text-ink">
                        {config.codNote}
                    </p>
                </section>
            ) : null}
        </div>
    )
}
```

- [ ] **Step 2: Verify**

Run: `npx tsc --noEmit 2>&1 | Measure-Object -Line` → 61.
Visual (if API up): `/` shows hero, category tiles, product strips, editorial block. Empty catalog → sections gracefully omitted.

- [ ] **Step 3: Commit**

```bash
git add "app/(store)/page.tsx"
git commit -m "feat(store): editorial home page

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 7: Product listing (PLP) — `/shop` and `/shop/[categoryId]`

**Files:**
- Create: `app/(store)/shop/page.tsx`
- Create: `app/(store)/shop/[categoryId]/page.tsx`
- Create: `app/(store)/shop/_components/ProductGrid.tsx`
- Create: `app/(store)/shop/_components/Pagination.tsx`

**Interfaces:**
- Consumes: `getStoreProducts`, `getStoreCategories` (Task 1); `ProductCard`, `SectionHeading` (Task 4).
- Produces: PLP routes. Next 16 async `searchParams`/`params`. Shared `ProductGrid` (4-col/2-col, 1px gaps, every 8th item a 2×2 editorial tile) and `Pagination`.

- [ ] **Step 1: ProductGrid**

`app/(store)/shop/_components/ProductGrid.tsx`:
```tsx
import type { StoreProductRow } from "../../_lib/store-types"
import { ProductCard } from "../../_components/ProductCard"

/**
 * 4-col (2-col mobile) grid with 1px hairline gaps. Every 8th product spans
 * 2×2 as an editorial tile (larger card, same data). Purely presentational.
 */
export function ProductGrid({ products }: { products: StoreProductRow[] }) {
    if (products.length === 0) {
        return <p className="store-mono py-16 text-center text-sm text-ink-soft">No products here yet.</p>
    }
    return (
        <div className="grid grid-cols-2 gap-px bg-line md:grid-cols-4">
            {products.map((p, i) => {
                const editorial = (i + 1) % 8 === 0
                return (
                    <div
                        key={p.id}
                        className={`bg-ground p-3 ${editorial ? "col-span-2 row-span-2 md:p-6" : ""}`}
                    >
                        <ProductCard product={p} priority={i < 4} />
                    </div>
                )
            })}
        </div>
    )
}
```

- [ ] **Step 2: Pagination**

`app/(store)/shop/_components/Pagination.tsx`:
```tsx
import Link from "next/link"

export function Pagination({
    basePath,
    pageIndex,
    pageSize,
    total,
    extraQuery = "",
}: {
    basePath: string
    pageIndex: number
    pageSize: number
    total: number
    extraQuery?: string
}) {
    const pageCount = Math.max(1, Math.ceil(total / pageSize))
    if (pageCount <= 1) return null
    const q = (p: number) => `${basePath}?page=${p}${extraQuery ? `&${extraQuery}` : ""}`
    return (
        <nav className="mt-12 flex items-center justify-between border-t border-line pt-6">
            {pageIndex > 1 ? (
                <Link href={q(pageIndex - 1)} className="store-eyebrow text-ink hover:text-store-accent">
                    ← Previous
                </Link>
            ) : (
                <span className="store-eyebrow text-line">← Previous</span>
            )}
            <span className="store-mono text-xs text-ink-soft">
                {pageIndex} / {pageCount}
            </span>
            {pageIndex < pageCount ? (
                <Link href={q(pageIndex + 1)} className="store-eyebrow text-ink hover:text-store-accent">
                    Next →
                </Link>
            ) : (
                <span className="store-eyebrow text-line">Next →</span>
            )}
        </nav>
    )
}
```

- [ ] **Step 3: `/shop` (all products)**

`app/(store)/shop/page.tsx`:
```tsx
import { getStoreProducts } from "../_lib/store-api"
import { SectionHeading } from "../_components/atoms"
import { ProductGrid } from "./_components/ProductGrid"
import { Pagination } from "./_components/Pagination"

const PAGE_SIZE = 24

export const metadata = { title: "Shop — all products" }

export default async function ShopPage({
    searchParams,
}: {
    searchParams: Promise<{ page?: string; search?: string }>
}) {
    const sp = await searchParams
    const pageIndex = Math.max(1, Number(sp.page) || 1)
    const searchText = sp.search?.trim() || undefined
    const { total, data } = await getStoreProducts({ pageIndex, pageSize: PAGE_SIZE, searchText })

    return (
        <div className="py-12">
            <SectionHeading eyebrow={`${total} items`} title={searchText ? `Search: ${searchText}` : "Everything"} className="mb-8" />
            <ProductGrid products={data} />
            <Pagination
                basePath="/shop"
                pageIndex={pageIndex}
                pageSize={PAGE_SIZE}
                total={total}
                extraQuery={searchText ? `search=${encodeURIComponent(searchText)}` : ""}
            />
        </div>
    )
}
```

- [ ] **Step 4: `/shop/[categoryId]`**

`app/(store)/shop/[categoryId]/page.tsx`:
```tsx
import { notFound } from "next/navigation"
import { getStoreCategories, getStoreProducts } from "../../_lib/store-api"
import { SectionHeading } from "../../_components/atoms"
import { ProductGrid } from "../_components/ProductGrid"
import { Pagination } from "../_components/Pagination"

const PAGE_SIZE = 24

export async function generateMetadata({ params }: { params: Promise<{ categoryId: string }> }) {
    const { categoryId } = await params
    const categories = await getStoreCategories()
    const cat = categories.find((c) => c.id === Number(categoryId))
    return { title: cat ? `${cat.title} — Shop` : "Shop" }
}

export default async function CategoryPage({
    params,
    searchParams,
}: {
    params: Promise<{ categoryId: string }>
    searchParams: Promise<{ page?: string }>
}) {
    const { categoryId } = await params
    const id = Number(categoryId)
    if (!Number.isInteger(id) || id <= 0) notFound()

    const [categories, sp] = await Promise.all([getStoreCategories(), searchParams])
    const category = categories.find((c) => c.id === id)
    // A category with zero published products is not returned by the API; treat
    // an unknown id as not-found (designed empty state via notFound()).
    if (!category) notFound()

    const pageIndex = Math.max(1, Number(sp.page) || 1)
    const { total, data } = await getStoreProducts({ categoryId: id, pageIndex, pageSize: PAGE_SIZE })

    return (
        <div className="py-12">
            <SectionHeading eyebrow={`${total} items`} title={category.title} className="mb-8" />
            <ProductGrid products={data} />
            <Pagination basePath={`/shop/${id}`} pageIndex={pageIndex} pageSize={PAGE_SIZE} total={total} />
        </div>
    )
}
```

- [ ] **Step 5: Verify**

Run: `npx tsc --noEmit 2>&1 | Measure-Object -Line` → 61.
Visual (if API up): `/shop` grids products with pagination; `/shop/<id>` filters; unknown id → not-found; every 8th tile is 2×2.

- [ ] **Step 6: Commit**

```bash
git add "app/(store)/shop"
git commit -m "feat(store): product listing pages with grid and pagination

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 8: Product detail (PDP) — `/product/[id]`

**Files:**
- Create: `app/(store)/product/[id]/page.tsx`
- Create: `app/(store)/_components/AddToBag.tsx`
- Create: `app/(store)/product/[id]/_components/Gallery.tsx`
- Create: `app/(store)/product/[id]/_components/SpecAccordions.tsx`

**Interfaces:**
- Consumes: `getStoreProduct`, `getStoreProducts` (related), `StoreProductDetail`, `StoreVariant` (Task 1); `useCart` (Task 3); `formatCurrency`, `availabilityLabel` (Task 3); `ProductCard` (Task 4).
- Produces: PDP route + the `AddToBag` client island (variant select + qty + add). `generateMetadata` and Product JSON-LD.

- [ ] **Step 1: AddToBag island**

`app/(store)/_components/AddToBag.tsx`:
```tsx
"use client"

import { useState } from "react"
import type { StoreProductDetail, StoreVariant } from "../_lib/store-types"
import { useCart } from "../_lib/cart-store"
import { availabilityLabel, formatCurrency } from "../_lib/format"

export function AddToBag({ product }: { product: StoreProductDetail }) {
    const addItem = useCart((s) => s.addItem)
    const variants = product.variants
    const [variantId, setVariantId] = useState<number | null>(variants[0]?.id ?? null)
    const [qty, setQty] = useState(1)
    const [added, setAdded] = useState(false)

    const selected: StoreVariant | undefined = variants.find((v) => v.id === variantId)
    const price = selected?.price ?? product.price
    const avail = selected
        ? availabilityLabel(selected.inStock, selected.lowStockQuantity)
        : { text: "Unavailable", accent: false }
    const canAdd = !!selected && selected.inStock

    const onAdd = () => {
        if (!selected) return
        addItem(
            {
                productVariantId: selected.id,
                productId: product.id,
                title: product.title,
                variantTitle: selected.title,
                sku: selected.sku,
                price: selected.price,
                imageUrl: product.images[0] ?? null,
            },
            qty,
        )
        setAdded(true)
        window.setTimeout(() => setAdded(false), 1800)
    }

    return (
        <div className="space-y-5">
            <div className="store-mono text-xl text-ink">{formatCurrency(price)}</div>

            {selected ? (
                <div className="store-mono text-xs text-ink-soft">
                    SKU {selected.sku} ·{" "}
                    <span className={avail.accent ? "text-store-accent" : "text-ink-soft"}>{avail.text}</span>
                </div>
            ) : null}

            {variants.length > 1 ? (
                <div>
                    <label className="store-eyebrow text-ink-soft" htmlFor="variant">
                        Option
                    </label>
                    <select
                        id="variant"
                        value={variantId ?? ""}
                        onChange={(e) => setVariantId(Number(e.target.value))}
                        className="mt-2 w-full border border-line bg-surface px-3 py-2 store-mono text-sm text-ink"
                    >
                        {variants.map((v) => (
                            <option key={v.id} value={v.id} disabled={!v.inStock}>
                                {v.title} {v.inStock ? "" : "— out of stock"}
                            </option>
                        ))}
                    </select>
                </div>
            ) : null}

            <div className="flex items-center gap-3">
                <label className="store-eyebrow text-ink-soft" htmlFor="qty">
                    Qty
                </label>
                <input
                    id="qty"
                    type="number"
                    min={1}
                    max={99}
                    value={qty}
                    onChange={(e) => setQty(Math.max(1, Math.min(99, Number(e.target.value) || 1)))}
                    className="w-20 border border-line bg-surface px-3 py-2 store-mono text-sm text-ink"
                />
            </div>

            <button
                type="button"
                onClick={onAdd}
                disabled={!canAdd}
                className="w-full border border-ink bg-ink px-6 py-3 store-eyebrow text-ground transition-colors hover:bg-store-accent hover:border-store-accent disabled:cursor-not-allowed disabled:opacity-40"
            >
                {added ? "Added to bag ✓" : canAdd ? "Add to bag" : "Out of stock"}
            </button>
        </div>
    )
}
```

- [ ] **Step 2: Gallery (left stacked scrolling)**

`app/(store)/product/[id]/_components/Gallery.tsx`:
```tsx
const PLACEHOLDER_CLASS = "aspect-[4/5] w-full bg-surface"

export function Gallery({ images, title, productId }: { images: string[]; title: string; productId: number }) {
    if (images.length === 0) {
        return <div className={PLACEHOLDER_CLASS} />
    }
    return (
        <div className="space-y-4">
            {images.map((src, i) => (
                <div key={i} className="aspect-[4/5] w-full bg-surface overflow-hidden">
                    <img
                        src={src}
                        alt={i === 0 ? title : ""}
                        loading={i === 0 ? "eager" : "lazy"}
                        className="h-full w-full object-cover"
                        style={i === 0 ? { viewTransitionName: `product-image-${productId}` } : undefined}
                    />
                </div>
            ))}
        </div>
    )
}
```

- [ ] **Step 3: Spec accordions (signature module)**

`app/(store)/product/[id]/_components/SpecAccordions.tsx`:
```tsx
"use client"

import { useState } from "react"
import type { StoreProductDetail } from "../../_lib/store-types"

/**
 * Mono spec-sheet accordions — the design's signature module. Populated from
 * the data we actually have (v1): description + a specs list derived from the
 * product/variant facts. No fabricated content.
 */
export function SpecAccordions({ product }: { product: StoreProductDetail }) {
    const sections: Array<{ label: string; body: React.ReactNode }> = []
    if (product.description) {
        sections.push({ label: "Description", body: <p className="whitespace-pre-line">{product.description}</p> })
    }
    sections.push({
        label: "Specifications",
        body: (
            <dl className="grid grid-cols-[auto_1fr] gap-x-6 gap-y-1">
                <dt className="text-ink-soft">Category</dt>
                <dd>{product.categoryTitle ?? "—"}</dd>
                <dt className="text-ink-soft">Options</dt>
                <dd>{product.variants.length}</dd>
            </dl>
        ),
    })
    sections.push({
        label: "Delivery & payment",
        body: <p>Cash on delivery. Delivery fee applied at checkout.</p>,
    })

    return (
        <div className="mt-10 border-t border-line">
            {sections.map((s, i) => (
                <Accordion key={i} label={s.label} defaultOpen={i === 0}>
                    {s.body}
                </Accordion>
            ))}
        </div>
    )
}

function Accordion({
    label,
    defaultOpen = false,
    children,
}: {
    label: string
    defaultOpen?: boolean
    children: React.ReactNode
}) {
    const [open, setOpen] = useState(defaultOpen)
    return (
        <div className="border-b border-line">
            <button
                type="button"
                onClick={() => setOpen((o) => !o)}
                aria-expanded={open}
                className="flex w-full items-center justify-between py-4 store-eyebrow text-ink"
            >
                {label}
                <span className="store-mono text-ink-soft">{open ? "–" : "+"}</span>
            </button>
            {open ? <div className="store-mono pb-5 text-sm leading-relaxed text-ink">{children}</div> : null}
        </div>
    )
}
```

- [ ] **Step 4: PDP page**

`app/(store)/product/[id]/page.tsx`:
```tsx
import { notFound } from "next/navigation"
import Link from "next/link"
import { getStoreProduct, getStoreProducts } from "../../_lib/store-api"
import { formatCurrency } from "../../_lib/format"
import { AddToBag } from "../../_components/AddToBag"
import { ProductCard } from "../../_components/ProductCard"
import { SectionHeading } from "../../_components/atoms"
import { Gallery } from "./_components/Gallery"
import { SpecAccordions } from "./_components/SpecAccordions"

export async function generateMetadata({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params
    const product = await getStoreProduct(Number(id))
    if (!product) return { title: "Product not found" }
    return {
        title: product.title,
        description: product.description ?? `${product.title} — available now.`,
        openGraph: { images: product.images.slice(0, 1) },
    }
}

export default async function ProductPage({ params }: { params: Promise<{ id: string }> }) {
    const { id } = await params
    const productId = Number(id)
    if (!Number.isInteger(productId) || productId <= 0) notFound()

    const product = await getStoreProduct(productId)
    if (!product) notFound()

    const related = (await getStoreProducts({ categoryId: product.categoryId, pageSize: 5 })).data.filter(
        (p) => p.id !== product.id,
    )

    const jsonLd = {
        "@context": "https://schema.org",
        "@type": "Product",
        name: product.title,
        description: product.description ?? undefined,
        image: product.images,
        sku: product.variants[0]?.sku,
        offers: {
            "@type": "Offer",
            price: product.price,
            priceCurrency: "NOK",
            availability: product.variants.some((v) => v.inStock)
                ? "https://schema.org/InStock"
                : "https://schema.org/OutOfStock",
        },
    }

    return (
        <div className="py-12">
            <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }} />

            <nav className="store-eyebrow mb-8 text-ink-soft">
                <Link href="/shop" className="hover:text-store-accent">
                    Shop
                </Link>
                {product.categoryTitle ? (
                    <>
                        {" / "}
                        <Link href={`/shop/${product.categoryId}`} className="hover:text-store-accent">
                            {product.categoryTitle}
                        </Link>
                    </>
                ) : null}
            </nav>

            <div className="grid grid-cols-1 gap-10 md:grid-cols-12">
                <div className="md:col-span-7">
                    <Gallery images={product.images} title={product.title} productId={product.id} />
                </div>
                <div className="md:col-span-5">
                    <div className="sticky top-24">
                        <h1 className="store-display text-[clamp(2rem,4vw,3.25rem)] text-ink">{product.title}</h1>
                        <div className="mt-6">
                            <AddToBag product={product} />
                        </div>
                        <SpecAccordions product={product} />
                    </div>
                </div>
            </div>

            {related.length > 0 ? (
                <section className="mt-24 border-t border-line pt-12">
                    <SectionHeading eyebrow="More from" title={product.categoryTitle ?? "This category"} className="mb-8" />
                    <div className="flex gap-4 overflow-x-auto pb-2">
                        {related.map((p) => (
                            <div key={p.id} className="w-[clamp(180px,22vw,240px)] shrink-0">
                                <ProductCard product={p} />
                            </div>
                        ))}
                    </div>
                </section>
            ) : null}
        </div>
    )
}
```

- [ ] **Step 5: Verify**

Run: `npx tsc --noEmit 2>&1 | Measure-Object -Line` → 61.
Visual (if API up): PDP renders gallery + sticky buy box; variant select changes price/availability; "Add to bag" ticks the header count; unknown id → not-found; JSON-LD present in the page source.

- [ ] **Step 6: Commit**

```bash
git add "app/(store)/product" "app/(store)/_components/AddToBag.tsx"
git commit -m "feat(store): product detail page with add-to-bag and spec accordions

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 9: Cart page — `/cart`

**Files:**
- Create: `app/(store)/cart/page.tsx`
- Create: `app/(store)/cart/_components/CartTable.tsx`

**Interfaces:**
- Consumes: `useCart`, `useCartSubtotal`, `useCartHydrated` (Task 3); `getStoreConfig` for the delivery fee (Task 1); `formatCurrency` (Task 3).
- Produces: `/cart` — a mono line-item order form.

- [ ] **Step 1: Cart page (server shell passes deliveryFee)**

`app/(store)/cart/page.tsx`:
```tsx
import { getStoreConfig } from "../_lib/store-api"
import { CartTable } from "./_components/CartTable"

export const metadata = { title: "Your bag" }

export default async function CartPage() {
    const config = await getStoreConfig()
    return (
        <div className="py-12">
            <h1 className="store-display mb-8 text-[clamp(2rem,5vw,4rem)] text-ink">Your bag</h1>
            <CartTable deliveryFee={config.deliveryFee} />
        </div>
    )
}
```

- [ ] **Step 2: CartTable island**

`app/(store)/cart/_components/CartTable.tsx`:
```tsx
"use client"

import Link from "next/link"
import { useCart, useCartHydrated, useCartSubtotal } from "../../_lib/cart-store"
import { formatCurrency } from "../../_lib/format"

export function CartTable({ deliveryFee }: { deliveryFee: number }) {
    const hydrated = useCartHydrated()
    const items = useCart((s) => s.items)
    const setQty = useCart((s) => s.setQty)
    const removeItem = useCart((s) => s.removeItem)
    const subtotal = useCartSubtotal()

    if (!hydrated) {
        return <p className="store-mono text-sm text-ink-soft">Loading your bag…</p>
    }
    if (items.length === 0) {
        return (
            <div className="border-t border-line py-16 text-center">
                <p className="store-mono text-sm text-ink-soft">Your bag is empty.</p>
                <Link href="/shop" className="store-eyebrow mt-4 inline-block text-store-accent">
                    Start shopping →
                </Link>
            </div>
        )
    }

    const total = subtotal + deliveryFee

    return (
        <div>
            <div className="border-t border-line">
                {/* header row */}
                <div className="hidden grid-cols-[1fr_auto_auto_auto] gap-6 border-b border-line py-3 store-eyebrow text-ink-soft md:grid">
                    <span>Item</span>
                    <span className="text-right">Price</span>
                    <span className="text-center">Qty</span>
                    <span className="text-right">Total</span>
                </div>
                {items.map((i) => (
                    <div
                        key={i.productVariantId}
                        className="grid grid-cols-[1fr_auto] items-center gap-4 border-b border-line py-4 md:grid-cols-[1fr_auto_auto_auto] md:gap-6"
                    >
                        <div className="flex items-center gap-4">
                            <div className="h-16 w-16 shrink-0 bg-surface overflow-hidden">
                                {i.imageUrl ? <img src={i.imageUrl} alt="" className="h-full w-full object-cover" /> : null}
                            </div>
                            <div>
                                <div className="text-sm text-ink">{i.title}</div>
                                <div className="store-mono text-xs text-ink-soft">
                                    {i.variantTitle} · SKU {i.sku}
                                </div>
                                <button
                                    type="button"
                                    onClick={() => removeItem(i.productVariantId)}
                                    className="store-eyebrow mt-1 text-ink-soft hover:text-store-accent"
                                >
                                    Remove
                                </button>
                            </div>
                        </div>
                        <div className="store-mono hidden text-right text-sm text-ink md:block">
                            {formatCurrency(i.price)}
                        </div>
                        <div className="md:text-center">
                            <input
                                type="number"
                                min={1}
                                max={99}
                                value={i.quantity}
                                onChange={(e) =>
                                    setQty(i.productVariantId, Math.max(1, Math.min(99, Number(e.target.value) || 1)))
                                }
                                aria-label={`Quantity for ${i.title}`}
                                className="w-16 border border-line bg-surface px-2 py-1 store-mono text-sm text-ink"
                            />
                        </div>
                        <div className="store-mono text-right text-sm text-ink">
                            {formatCurrency(i.price * i.quantity)}
                        </div>
                    </div>
                ))}
            </div>

            {/* totals */}
            <div className="mt-8 ml-auto max-w-sm space-y-2 store-mono text-sm">
                <div className="flex justify-between text-ink-soft">
                    <span>Subtotal</span>
                    <span>{formatCurrency(subtotal)}</span>
                </div>
                <div className="flex justify-between text-ink-soft">
                    <span>Delivery</span>
                    <span>{formatCurrency(deliveryFee)}</span>
                </div>
                <div className="flex justify-between border-t border-line pt-2 text-ink">
                    <span>Total</span>
                    <span>{formatCurrency(total)}</span>
                </div>
                <Link
                    href="/checkout"
                    className="mt-4 block border border-ink bg-ink px-6 py-3 text-center store-eyebrow text-ground transition-colors hover:bg-store-accent hover:border-store-accent"
                >
                    Checkout
                </Link>
            </div>
        </div>
    )
}
```

- [ ] **Step 3: Verify**

Run: `npx tsc --noEmit 2>&1 | Measure-Object -Line` → 61.
Visual (if API up): add items on a PDP → `/cart` lists them; qty edit updates line + subtotal; remove works; delivery fee + total shown; empty state after clearing.

- [ ] **Step 4: Commit**

```bash
git add "app/(store)/cart"
git commit -m "feat(store): cart page with line-item order form

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 10: Checkout + confirmation — `/checkout`, `/order/[number]`

**Files:**
- Create: `app/(store)/checkout/page.tsx`
- Create: `app/(store)/checkout/_components/CheckoutForm.tsx`
- Create: `app/(store)/order/[number]/page.tsx`

**Interfaces:**
- Consumes: `useCart`, `useCartSubtotal`, `useCartHydrated`, `clear` (Task 3); `getStoreConfig` (Task 1); `placeOrder`, `ValidationError` (Task 1, `store-client.ts`); `formatCurrency` (Task 3); zod + react-hook-form.
- Produces: `/checkout` (single 560px column, numbered sections, honeypot) and `/order/[number]` (giant mono order number).

- [ ] **Step 1: Checkout server shell**

`app/(store)/checkout/page.tsx`:
```tsx
import { getStoreConfig } from "../_lib/store-api"
import { CheckoutForm } from "./_components/CheckoutForm"

export const metadata = { title: "Checkout" }

export default async function CheckoutPage() {
    const config = await getStoreConfig()
    return (
        <div className="py-12">
            <div className="mx-auto max-w-[560px]">
                <h1 className="store-display mb-8 text-[clamp(2rem,5vw,3.5rem)] text-ink">Checkout</h1>
                <CheckoutForm deliveryFee={config.deliveryFee} codNote={config.codNote} />
            </div>
        </div>
    )
}
```

- [ ] **Step 2: CheckoutForm island**

`app/(store)/checkout/_components/CheckoutForm.tsx`:
```tsx
"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import Link from "next/link"
import { useCart, useCartHydrated, useCartSubtotal } from "../../_lib/cart-store"
import { formatCurrency } from "../../_lib/format"
import { placeOrder, ValidationError } from "../../_lib/store-client"

// Mirrors the backend validator (CreateOnlineOrderCommandValidator).
const schema = z.object({
    customerName: z.string().trim().min(1, "Name is required").max(200),
    phone: z
        .string()
        .trim()
        .min(6, "Phone number looks invalid")
        .max(32)
        .regex(/^[0-9+\-\s()]{6,}$/, "Phone number looks invalid"),
    address: z.string().trim().min(1, "Address is required").max(1000),
    note: z.string().trim().max(1000).optional(),
    website: z.string().max(0).optional(), // honeypot: must stay empty
})

type FormValues = z.infer<typeof schema>

export function CheckoutForm({ deliveryFee, codNote }: { deliveryFee: number; codNote: string }) {
    const router = useRouter()
    const hydrated = useCartHydrated()
    const items = useCart((s) => s.items)
    const clear = useCart((s) => s.clear)
    const subtotal = useCartSubtotal()
    const [serverErrors, setServerErrors] = useState<string[]>([])
    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
    } = useForm<FormValues>({ resolver: zodResolver(schema) })

    if (hydrated && items.length === 0) {
        return (
            <div className="border-t border-line py-16 text-center">
                <p className="store-mono text-sm text-ink-soft">Your bag is empty.</p>
                <Link href="/shop" className="store-eyebrow mt-4 inline-block text-store-accent">
                    Start shopping →
                </Link>
            </div>
        )
    }

    const onSubmit = async (values: FormValues) => {
        setServerErrors([])
        try {
            const result = await placeOrder({
                customerName: values.customerName,
                phone: values.phone,
                address: values.address,
                note: values.note || undefined,
                website: values.website || undefined,
                items: items.map((i) => ({ productVariantId: i.productVariantId, quantity: i.quantity })),
            })
            clear()
            router.push(`/order/${encodeURIComponent(result.orderNumber)}`)
        } catch (err) {
            if (err instanceof ValidationError) setServerErrors(err.messages)
            else setServerErrors([err instanceof Error ? err.message : "Something went wrong. Please try again."])
        }
    }

    const total = subtotal + deliveryFee

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-10" noValidate>
            <Section index="01" title="Contact">
                <Field label="Full name" error={errors.customerName?.message}>
                    <input {...register("customerName")} className={inputClass} autoComplete="name" />
                </Field>
                <Field label="Phone" error={errors.phone?.message}>
                    <input {...register("phone")} className={inputClass} inputMode="tel" autoComplete="tel" />
                </Field>
            </Section>

            <Section index="02" title="Delivery">
                <Field label="Address" error={errors.address?.message}>
                    <textarea {...register("address")} rows={3} className={inputClass} autoComplete="street-address" />
                </Field>
                <Field label="Note (optional)" error={errors.note?.message}>
                    <textarea {...register("note")} rows={2} className={inputClass} />
                </Field>
            </Section>

            <Section index="03" title="Payment">
                <p className="store-mono text-sm text-ink-soft">
                    Cash on delivery. {codNote}
                </p>
            </Section>

            {/* Honeypot: hidden from humans, tabbable-off; bots fill it and the server drops the order. */}
            <div aria-hidden className="absolute left-[-9999px] h-0 w-0 overflow-hidden">
                <label>
                    Do not fill this
                    <input {...register("website")} tabIndex={-1} autoComplete="off" />
                </label>
            </div>

            <div className="border-t border-line pt-6 store-mono text-sm">
                <div className="flex justify-between text-ink-soft">
                    <span>Subtotal</span>
                    <span>{formatCurrency(subtotal)}</span>
                </div>
                <div className="flex justify-between text-ink-soft">
                    <span>Delivery</span>
                    <span>{formatCurrency(deliveryFee)}</span>
                </div>
                <div className="mt-1 flex justify-between border-t border-line pt-2 text-ink">
                    <span>Total (COD)</span>
                    <span>{formatCurrency(total)}</span>
                </div>
            </div>

            {serverErrors.length > 0 ? (
                <ul className="border border-store-accent/40 bg-store-accent/5 p-3 store-mono text-xs text-store-accent">
                    {serverErrors.map((m, i) => (
                        <li key={i}>{m}</li>
                    ))}
                </ul>
            ) : null}

            <button
                type="submit"
                disabled={isSubmitting || !hydrated}
                className="w-full border border-ink bg-ink px-6 py-3 store-eyebrow text-ground transition-colors hover:bg-store-accent hover:border-store-accent disabled:opacity-40"
            >
                {isSubmitting ? "Placing order…" : "Place order"}
            </button>
        </form>
    )
}

const inputClass =
    "mt-2 w-full border border-line bg-surface px-3 py-2 store-mono text-sm text-ink focus:border-ink"

function Section({ index, title, children }: { index: string; title: string; children: React.ReactNode }) {
    return (
        <section>
            <div className="mb-4 flex items-baseline gap-3 border-b border-line pb-2">
                <span className="store-mono text-store-accent">{index}</span>
                <h2 className="store-eyebrow text-ink">{title}</h2>
            </div>
            <div className="space-y-4">{children}</div>
        </section>
    )
}

function Field({ label, error, children }: { label: string; error?: string; children: React.ReactNode }) {
    return (
        <div>
            <label className="store-eyebrow text-ink-soft">{label}</label>
            {children}
            {error ? <p className="store-mono mt-1 text-xs text-store-accent">{error}</p> : null}
        </div>
    )
}
```

**Dependency check:** `@hookform/resolvers` must be installed (it provides `zodResolver`). Run `node -e "require('@hookform/resolvers/zod')"` — if it errors, check how existing dashboard forms wire zod (grep `zodResolver` under `app/(dashboard)`); the repo already uses react-hook-form + zod so the resolver is very likely present. If it is genuinely absent, match the existing pattern the dashboard uses rather than adding a dependency.

- [ ] **Step 3: Confirmation page**

`app/(store)/order/[number]/page.tsx`:
```tsx
import Link from "next/link"

export const metadata = { title: "Order confirmed" }

export default async function OrderConfirmationPage({ params }: { params: Promise<{ number: string }> }) {
    const { number } = await params
    const orderNumber = decodeURIComponent(number)
    return (
        <div className="flex min-h-[60vh] flex-col items-center justify-center py-20 text-center">
            <div className="store-eyebrow text-ink-soft">Order received</div>
            <div className="store-mono mt-6 text-[clamp(2.5rem,10vw,7rem)] leading-none text-ink">{orderNumber}</div>
            <p className="store-mono mt-6 max-w-[42ch] text-sm text-ink-soft">
                Thank you. We&apos;ll call you to confirm delivery and collect payment on arrival (cash on delivery).
            </p>
            <Link href="/shop" className="store-eyebrow mt-10 inline-block text-store-accent">
                Continue shopping →
            </Link>
        </div>
    )
}
```

**Note:** the confirmation page intentionally does NOT fetch the order (guest orders are not publicly retrievable — no public GET-by-number endpoint, by design). It only echoes the number the checkout returned. This matches the spec's out-of-scope "order tracking beyond the confirmation page."

- [ ] **Step 4: Verify**

Run: `npx tsc --noEmit 2>&1 | Measure-Object -Line` → 61.
Visual (if API up): fill checkout → invalid phone shows inline error (client zod) and, if the server rejects, the 422 messages list; a valid submit routes to `/order/W00000X` with the giant mono number and an emptied cart. Filling the honeypot (via devtools) → server returns `W000000` and the cart still clears (acceptable; bots don't reach the confirmation meaningfully).

- [ ] **Step 5: Commit**

```bash
git add "app/(store)/checkout" "app/(store)/order"
git commit -m "feat(store): checkout form and order confirmation

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 11: Motion, SEO/ISR polish, and final end-to-end verification

**Files:**
- Modify: `next.config.ts` (enable view transitions)
- Create: `app/(store)/_components/Reveal.tsx`
- Create: `app/sitemap.ts`
- Create: `app/robots.ts`
- Modify: home/PLP/PDP to wrap sections in `Reveal` (light touch)

**Interfaces:**
- Consumes: everything prior.
- Produces: the four shipped motions that are safe to add now (marquee already done in Task 4; card hover done in Task 4; scroll reveals here; add-to-bag feedback done in Task 8), the view-transition config, and SEO artifacts.

- [ ] **Step 1: Enable view transitions (experimental)**

Read `next.config.ts` first. Add the experimental flag (Next 16):
```ts
// inside the nextConfig object
experimental: {
    viewTransition: true,
},
```
The `ProductCard` and PDP `Gallery` already set `viewTransitionName: product-image-<id>`; with the flag on, Next's App Router applies a cross-document/route morph on navigation. **This is bleeding-edge** — if enabling it breaks the build or throws, leave the CSS `viewTransitionName` in place (harmless) and disable the flag; the feature degrades to a normal navigation. Record the outcome.

- [ ] **Step 2: Scroll-reveal wrapper (CSS-first, reduced-motion safe)**

`app/(store)/_components/Reveal.tsx`:
```tsx
"use client"

import { useEffect, useRef, useState } from "react"

/**
 * Opacity + 16px translateY reveal on scroll. Prefers the native CSS
 * scroll-driven animation where supported; falls back to IntersectionObserver.
 * The reduced-motion rule in store-tokens.css neutralizes it for opt-outs.
 */
export function Reveal({ children, className = "" }: { children: React.ReactNode; className?: string }) {
    const ref = useRef<HTMLDivElement>(null)
    const [shown, setShown] = useState(false)
    useEffect(() => {
        const el = ref.current
        if (!el) return
        const io = new IntersectionObserver(
            (entries) => {
                for (const e of entries) if (e.isIntersecting) setShown(true)
            },
            { rootMargin: "0px 0px -10% 0px" },
        )
        io.observe(el)
        return () => io.disconnect()
    }, [])
    return (
        <div
            ref={ref}
            className={className}
            style={{
                opacity: shown ? 1 : 0,
                transform: shown ? "translateY(0)" : "translateY(16px)",
                transition: "opacity 600ms ease, transform 600ms ease",
            }}
        >
            {children}
        </div>
    )
}
```
Apply lightly: wrap the home page's category-tiles section and each product strip, and the PLP grid, in `<Reveal>`. Do NOT wrap above-the-fold hero content (it should paint immediately). Keep edits minimal — one import + wrapping a few sections.

- [ ] **Step 3: SEO artifacts**

`app/sitemap.ts` (root — outside the group so it serves at `/sitemap.xml`):
```ts
import type { MetadataRoute } from "next"
import { getStoreCategories, getStoreProducts } from "./(store)/_lib/store-api"

const SITE = process.env.NEXT_PUBLIC_SITE_URL || "http://localhost:3000"

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
    try {
        const categories = await getStoreCategories()
        // First page of products is enough for v1 discovery.
        const products = (await getStoreProducts({ pageSize: 60 })).data
        return [
            { url: `${SITE}/`, priority: 1 },
            { url: `${SITE}/shop`, priority: 0.8 },
            ...categories.map((c) => ({ url: `${SITE}/shop/${c.id}`, priority: 0.6 })),
            ...products.map((p) => ({ url: `${SITE}/product/${p.id}`, priority: 0.5 })),
        ]
    } catch {
        // Store closed / API down: minimal sitemap.
        return [{ url: `${SITE}/`, priority: 1 }]
    }
}
```

`app/robots.ts`:
```ts
import type { MetadataRoute } from "next"

const SITE = process.env.NEXT_PUBLIC_SITE_URL || "http://localhost:3000"

export default function robots(): MetadataRoute.Robots {
    return {
        rules: { userAgent: "*", allow: "/", disallow: ["/dashboard", "/settings", "/api"] },
        sitemap: `${SITE}/sitemap.xml`,
    }
}
```

- [ ] **Step 4: Full frontend verification**

Run: `cd C:\Personal\nexterp\NextErp_React && npx tsc --noEmit 2>&1 | Select-String "error TS" | Measure-Object -Line`
Expected: **61** — the same baseline; zero new errors in any `app/(store)/**`, `app/sitemap.ts`, `app/robots.ts`, or `next.config.ts` file.
Also run a production build to catch RSC/ISR issues tsc can't: `npm run build`. Expected: build succeeds; store routes appear as ISR/SSG in the output. If `next build` reveals errors, fix them before proceeding (build must be green).

- [ ] **Step 5: Manual end-to-end smoke (requires API + dev server)**

With the API up (`StorefrontEnabled=true`, `SellingBranchId` set, ≥1 published category/product with stock, seeded from Plan 2's `/settings/ecommerce`) and `npm run dev`:
1. `/` renders hero/tiles/strips; disabled flag → closed page.
2. Browse `/shop` → paginate → open a PDP → select variant → Add to bag (header count ticks).
3. `/cart` → adjust qty/remove → Checkout.
4. Submit with a bad phone → inline error; fix → Place order → `/order/W00000X` with the cart cleared.
5. In the ERP admin: `/ecommerce/online-orders` shows the new Pending order; **Confirm** it → a Sale is created and linked (stock moves); **or** Cancel with a reason. Verify the confirm success/failure paths from Plan 2 behave.
Document observations. If the API cannot run here, record the smoke as deferred and rely on the tsc + `next build` gates + the per-task reviews.

- [ ] **Step 6: Final commit + push**

```bash
git add next.config.ts "app/(store)/_components/Reveal.tsx" app/sitemap.ts app/robots.ts "app/(store)"
git commit -m "feat(store): motion, scroll reveals, and SEO artifacts

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
git push origin main
```

---

## Plan self-review notes (applied)

- **Spec coverage:** root-route reconciliation (store owns `/`) = Task 0 ✓; hero/marquee/tiles/strips/editorial = Task 6 ✓; PLP 4-col + editorial tiles + hover crossfade = Tasks 4,7 ✓; PDP gallery + sticky buy box + mono price/SKU/availability + spec accordions + related = Task 8 ✓; cart order-form = Task 9 ✓; checkout numbered sections + honeypot + COD + confirmation giant mono number = Task 10 ✓; closed-state page = Task 5 ✓; tokens/fonts/anti-rules = Task 2 ✓; motion (5 items: marquee T4, card hover T4, scroll reveals T11, view-transition T11, add-to-bag T8) ✓; ISR + generateMetadata + JSON-LD + sitemap + robots = Tasks 6–8,11 ✓; cart persistence + hydration gate = Task 3 ✓; anonymous server fetch + 403/422/429 handling = Tasks 1,10 ✓; empty states throughout ✓.
- **Deviations (intentional, documented):** the storefront owns `/` and the authenticated home moves to `/dashboard` (owner-approved, Task 0 — the only admin-shell change); category route uses numeric `[categoryId]` (backend has no slug); store design tokens are registered under unique names (`ground`/`ink`/`store-accent`/…) so the admin `--color-accent` is never redefined (Task 2); images use `<img>` not `next/image` (avoid remotePatterns churn); `opengraph-image` is covered via per-product `openGraph.images` in `generateMetadata` rather than a generated OG image file (v1 simplification — flag if the owner wants a branded OG template); confirmation page echoes the number without a public order-fetch (no such endpoint by design); `NEXT_PUBLIC_SITE_URL` is undefined and falls back to localhost — must be set in production for correct sitemap/robots URLs.
- **Applied from the plan review (2026-07-03):** (Critical) resolved the `/` route collision with the existing `app/(dashboard)/page.tsx` via Task 0; (Critical) fixed Tailwind token registration so store utilities actually emit and the admin accent is untouched (Task 2); (Critical) fixed the anonymous `/login` bounce caused by the ungated `CurrencyLocaleSync` feature-settings fetch (Task 0, correcting the earlier AuthProvider misdiagnosis); (Important) added `store-errors.ts` so `StoreClosedError` is defined in the client module after the server/client split (Task 1); (Minor) floor decimal low-stock for display (Task 3).
- **Known execution checkpoints (verify, don't assume):** Task 0 login/redirect relocation must catch every "authenticated home" link (Task 0 Step 2 grep); anonymous store routes must not bounce or toast (Task 5 checkpoint 1); `.store-scope` must override the root `theme-zinc` body background (Task 5 checkpoint 2); store utilities must actually emit from the appended `globals.css` `@theme` (verified visually in Task 5 / `next build` in Task 11); `viewTransitionName` typing on `CSSProperties` (Task 4); `@hookform/resolvers` presence (Task 10); `experimental.viewTransition` build stability (Task 11).
- **Testing adaptation:** no unit-test runner exists; every task gates on `tsc --noEmit` (baseline 61, zero new) + manual verification, and Task 11 adds a `next build` gate + full E2E smoke. This matches the design spec's stated frontend testing approach. Note `next.config.ts` has `typescript.ignoreBuildErrors:true`, so `next build` (Task 11) is the real router/RSC gate — several defects (route collision, missing utilities) are invisible to `tsc` alone.
- **Type consistency:** the DTO field names in Task 1 (`imageUrl`, `secondImageUrl`, `lowStockQuantity`, `hasVariations`, `orderNumber`, `productVariantId`) are used verbatim in every consuming task; `CartLine` shape defined in Task 3 is consumed unchanged by Tasks 8–10; error classes live in `store-errors.ts` and are imported by both `store-api.ts` and `store-client.ts`.

---

## Execution options

**Plan complete and saved to `docs/superpowers/plans/2026-07-03-ecommerce-storefront.md`. Two execution options:**

**1. Subagent-Driven (recommended)** — a fresh subagent per task, two-stage review between tasks, fast iteration.

**2. Inline Execution** — execute tasks in this session with checkpoints for review.
