# Ecommerce Admin UI (Plan 2 of 3) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** ERP admin UI for the storefront: a category/product **publication manager** (the "manually select/unselect category and products" the owner asked for) under Settings, and an **Online Orders** page to review, confirm, and cancel guest orders — all against the Plan 1 backend.

**Architecture:** `NextErp_React` (Next.js 16 App Router). Follows the repo's strict 3-layer server-state convention (`lib/api/<entity>.ts` → `lib/query/keys.ts` + `options.ts` → `hooks/use-<entity>.ts` → page). Store-config settings (name/hero/fee) need NO new UI — the existing generic renderer at `/settings/features` auto-displays the backend `Ecommerce` settings module. Nav entries are DB Module rows (seeded), not frontend code.

**Tech Stack:** Next.js 16, TypeScript, shadcn/ui, TanStack Query v5, react-hook-form + zod, sonner, date-fns, Tailwind v4.

## Global Constraints

- **No new TS errors.** Verify each task with the project type-checker filtered to the files it touched: `cd C:\Personal\nexterp\NextErp_React && node_modules\.bin\tsc.cmd -p tsconfig.json --noEmit`. The repo has KNOWN pre-existing errors (e.g. `product-form.tsx` RHF resolver/Control, `customer-form.tsx`, `identity.ts`, `.next/types`); a task passes if it adds ZERO new error lines in the files it created/modified. Establish the baseline error set before Task 1 and diff against it.
- **3-layer convention (CONTRIBUTING.md):** components never import `@tanstack/react-query` directly; go api → keys → options → hook → page. All api fns accept an optional `AbortSignal`.
- **Meta-driven toasts:** mutations declare `meta: { successMessage, invalidates }` (from `lib/query/client.ts` `MutationMeta`); never hand-roll try/catch toasts for the happy path. 422 validation errors are already silent globally — map to forms via `applyValidationErrors` (`lib/query/rhf.ts`).
- **Backend JSON is camelCase.** All response fields arrive camelCased (`orderNumber`, `isPublishedOnline`, etc.).
- **Backend contracts (from Plan 1, exact):**
  - `GET api/onlineorder?status=&pageIndex=&pageSize=` → `{ total, data: OnlineOrderRow[] }`, `OnlineOrderRow = { id, orderNumber, customerName, phone, itemCount, itemsTotal, deliveryFee, status, createdAt }`. `status` is a string enum: `"Pending" | "Confirmed" | "Cancelled"`.
  - `GET api/onlineorder/{id}` → `OnlineOrderDetail = { id, orderNumber, customerName, phone, address, note, status, cancelReason, deliveryFee, partyId, saleId, createdAt, confirmedAt, items: OnlineOrderItemRow[] }`, `OnlineOrderItemRow = { productTitle, sku, unitPrice, quantity, lineTotal }`. 404 when missing.
  - `POST api/onlineorder/{id}/confirm` → `{ saleId }`. May 4xx (insufficient stock / wrong branch) — surface the ProblemDetails message.
  - `POST api/onlineorder/{id}/cancel` body `{ reason }` → 204.
  - `GET api/ecommerce/publication` → `PublicationCategory[] = { id, title, parentId, isPublishedOnline, products: [{ id, title, code, price, isPublishedOnline }] }`.
  - `PUT api/ecommerce/publication` body `{ publishCategoryIds, unpublishCategoryIds, publishProductIds, unpublishProductIds }` → 204.
- **Auth/role:** `/settings/*` pages inherit the admin gate in `app/(dashboard)/settings/layout.tsx`. The publication page additionally uses `useRequirePermission("Settings.System.Manage")` (matching `settings/features/page.tsx`). Online Orders page is `[Authorize]`-level (any authenticated user), like other operational pages.
- **Currency/date helpers:** money via `formatCurrency` (`lib/formatters/currency.ts`), dates via `formatDateTime`/`formatDateTimeWithTime` (`lib/formatters/date.ts`). Never hardcode `$`/separators.
- Windows: run `tsc` via `node_modules\.bin\tsc.cmd`. Frontend dev server host is `127.0.0.1:3000`; API at `http://localhost:5039`. Commit messages end with `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>`. Work on `main` (owner consent). Do NOT push until the final task.

---

### Task 1: Server-state layer — API clients, types, keys, options

**Files:**
- Create: `lib/types/online-order.ts`
- Create: `lib/types/ecommerce-publication.ts`
- Create: `lib/api/online-order.ts`
- Create: `lib/api/ecommerce.ts`
- Modify: `lib/query/keys.ts` (append two key families)
- Modify: `lib/query/options.ts` (append two query-option groups)

**Interfaces:**
- Produces types: `OnlineOrderRow`, `OnlineOrderStatus` (`"Pending"|"Confirmed"|"Cancelled"`), `OnlineOrderDetail`, `OnlineOrderItemRow`, `OnlineOrderListResponse` (`{ total, data }`); `PublicationCategory`, `PublicationProduct`, `PublicationUpdate`.
- Produces api: `onlineOrderAPI.{ getOrders(filters, signal), getOrder(id, signal), confirm(id), cancel(id, reason) }`; `ecommerceAPI.{ getPublication(signal), setPublication(update) }`.
- Produces keys: `queryKeys.onlineOrders.{ all, lists(), list(filters), details(), detail(id) }`; `queryKeys.ecommerce.{ all, publication() }`.
- Produces options: `onlineOrderQueries.{ list(filters), detail(id) }`; `ecommerceQueries.{ publication() }`.

- [ ] **Step 1: Establish the tsc baseline**

Run: `cd C:\Personal\nexterp\NextErp_React && node_modules\.bin\tsc.cmd -p tsconfig.json --noEmit 2>&1 | Select-String "error TS" | Measure-Object` — record the count and the set of file paths. This is the baseline; later steps must not add file paths to it.

- [ ] **Step 2: Types**

`lib/types/online-order.ts`:
```typescript
export type OnlineOrderStatus = "Pending" | "Confirmed" | "Cancelled"

export interface OnlineOrderRow {
    id: number
    orderNumber: string
    customerName: string
    phone: string
    itemCount: number
    itemsTotal: number
    deliveryFee: number
    status: OnlineOrderStatus
    createdAt: string
}

export interface OnlineOrderItemRow {
    productTitle: string
    sku: string
    unitPrice: number
    quantity: number
    lineTotal: number
}

export interface OnlineOrderDetail {
    id: number
    orderNumber: string
    customerName: string
    phone: string
    address: string
    note?: string | null
    status: OnlineOrderStatus
    cancelReason?: string | null
    deliveryFee: number
    partyId?: string | null
    saleId?: string | null
    createdAt: string
    confirmedAt?: string | null
    items: OnlineOrderItemRow[]
}

export interface OnlineOrderListResponse {
    total: number
    data: OnlineOrderRow[]
}

export interface OnlineOrderListFilters {
    pageIndex: number
    pageSize: number
    status?: OnlineOrderStatus | null
}
```

`lib/types/ecommerce-publication.ts`:
```typescript
export interface PublicationProduct {
    id: number
    title: string
    code: string
    price: number
    isPublishedOnline: boolean
}

export interface PublicationCategory {
    id: number
    title: string
    parentId?: number | null
    isPublishedOnline: boolean
    products: PublicationProduct[]
}

export interface PublicationUpdate {
    publishCategoryIds: number[]
    unpublishCategoryIds: number[]
    publishProductIds: number[]
    unpublishProductIds: number[]
}
```

- [ ] **Step 3: API clients** (mirror `lib/api/sale.ts` — `fetchAPI`, URLSearchParams, AbortSignal)

`lib/api/online-order.ts`:
```typescript
import { fetchAPI } from "@/lib/api/client"
import type {
    OnlineOrderDetail,
    OnlineOrderListFilters,
    OnlineOrderListResponse,
} from "@/lib/types/online-order"

export const onlineOrderAPI = {
    async getOrders(filters: OnlineOrderListFilters, signal?: AbortSignal): Promise<OnlineOrderListResponse> {
        const params = new URLSearchParams({
            pageIndex: filters.pageIndex.toString(),
            pageSize: filters.pageSize.toString(),
        })
        if (filters.status) params.append("status", filters.status)
        const raw = await fetchAPI<Record<string, unknown>>(`/api/OnlineOrder?${params.toString()}`, { signal })
        return {
            total: Number(raw?.total ?? 0),
            data: (Array.isArray(raw?.data) ? raw.data : []) as OnlineOrderListResponse["data"],
        }
    },

    async getOrder(id: number, signal?: AbortSignal): Promise<OnlineOrderDetail> {
        return fetchAPI<OnlineOrderDetail>(`/api/OnlineOrder/${id}`, { signal })
    },

    async confirm(id: number): Promise<{ saleId: string }> {
        return fetchAPI<{ saleId: string }>(`/api/OnlineOrder/${id}/confirm`, { method: "POST" })
    },

    async cancel(id: number, reason: string): Promise<void> {
        await fetchAPI<void>(`/api/OnlineOrder/${id}/cancel`, {
            method: "POST",
            body: JSON.stringify({ reason }),
        })
    },
}
```

`lib/api/ecommerce.ts`:
```typescript
import { fetchAPI } from "@/lib/api/client"
import type { PublicationCategory, PublicationUpdate } from "@/lib/types/ecommerce-publication"

export const ecommerceAPI = {
    async getPublication(signal?: AbortSignal): Promise<PublicationCategory[]> {
        const raw = await fetchAPI<PublicationCategory[]>(`/api/Ecommerce/publication`, { signal })
        return Array.isArray(raw) ? raw : []
    },

    async setPublication(update: PublicationUpdate): Promise<void> {
        await fetchAPI<void>(`/api/Ecommerce/publication`, {
            method: "PUT",
            body: JSON.stringify(update),
        })
    },
}
```
Note: confirm `fetchAPI`'s signature in `lib/api/client.ts` — if a `void` return trips it (empty 204 body), mirror however `customerAPI.deactivate`/similar handle 204 responses. Report if you adapt.

- [ ] **Step 4: Query keys** — append inside the `queryKeys` object in `lib/query/keys.ts`, mirroring the `sales`/`purchases` shape exactly:
```typescript
    onlineOrders: {
        all: ["onlineOrders"] as const,
        lists: () => [...queryKeys.onlineOrders.all, "list"] as const,
        list: (filters: unknown) => [...queryKeys.onlineOrders.lists(), filters] as const,
        details: () => [...queryKeys.onlineOrders.all, "detail"] as const,
        detail: (id: number | string) => [...queryKeys.onlineOrders.details(), id] as const,
    },
    ecommerce: {
        all: ["ecommerce"] as const,
        publication: () => [...queryKeys.ecommerce.all, "publication"] as const,
    },
```
(Match the exact `as const`/spread style already used in that file; if the file uses a different helper shape, follow it.)

- [ ] **Step 5: Query options** — append to `lib/query/options.ts`:
```typescript
// ---------------- Online Orders ----------------
import { onlineOrderAPI } from "@/lib/api/online-order"
import { ecommerceAPI } from "@/lib/api/ecommerce"
import type { OnlineOrderListFilters } from "@/lib/types/online-order"

export const onlineOrderQueries = {
    list: (filters: OnlineOrderListFilters) =>
        queryOptions({
            queryKey: queryKeys.onlineOrders.list(filters),
            queryFn: ({ signal }) => onlineOrderAPI.getOrders(filters, signal),
        }),
    detail: (id: number | string) =>
        queryOptions({
            queryKey: queryKeys.onlineOrders.detail(id),
            queryFn: ({ signal }) => onlineOrderAPI.getOrder(Number(id), signal),
            enabled: !!id,
        }),
} as const

export const ecommerceQueries = {
    publication: () =>
        queryOptions({
            queryKey: queryKeys.ecommerce.publication(),
            queryFn: ({ signal }) => ecommerceAPI.getPublication(signal),
        }),
} as const
```
(Move the `import` lines to the top of the file with the other imports — shown inline here only for locality.)

- [ ] **Step 6: Verify** — run the tsc command from Step 1. Expected: no new error lines in the 6 created/modified files.

- [ ] **Step 7: Commit**
```powershell
git add lib/types/online-order.ts lib/types/ecommerce-publication.ts lib/api/online-order.ts lib/api/ecommerce.ts lib/query/keys.ts lib/query/options.ts
git commit -m "feat(ecommerce-ui): server-state layer for online orders and publication

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

---

### Task 2: Hooks

**Files:**
- Create: `hooks/use-online-orders.ts`
- Create: `hooks/use-ecommerce-publication.ts`

**Interfaces:**
- Consumes: `onlineOrderQueries`, `ecommerceQueries`, `queryKeys`, mutation `meta`.
- Produces: `useOnlineOrders(filters)`, `useOnlineOrder(id)`, `useConfirmOnlineOrder()`, `useCancelOnlineOrder()`; `useEcommercePublication()`, `useSetEcommercePublication()`.

- [ ] **Step 1: Online-order hooks** (mirror `hooks/use-purchases.ts`: list adds `placeholderData: keepPreviousData`; mutations declare `meta`)

`hooks/use-online-orders.ts`:
```typescript
"use client"

import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { onlineOrderQueries } from "@/lib/query/options"
import { queryKeys } from "@/lib/query/keys"
import { onlineOrderAPI } from "@/lib/api/online-order"
import type { OnlineOrderListFilters } from "@/lib/types/online-order"

export function useOnlineOrders(filters: OnlineOrderListFilters) {
    return useQuery({ ...onlineOrderQueries.list(filters), placeholderData: keepPreviousData })
}

export function useOnlineOrder(id: number | string) {
    return useQuery(onlineOrderQueries.detail(id))
}

export function useConfirmOnlineOrder() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: number) => onlineOrderAPI.confirm(id),
        meta: {
            successMessage: "Order confirmed and sale created",
            invalidates: [queryKeys.onlineOrders.all, queryKeys.sales.all, queryKeys.stock.all],
        },
        // detail cache for this id must refresh too:
        onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.onlineOrders.all }),
    })
}

export function useCancelOnlineOrder() {
    return useMutation({
        mutationFn: ({ id, reason }: { id: number; reason: string }) => onlineOrderAPI.cancel(id, reason),
        meta: {
            successMessage: "Order cancelled",
            invalidates: [queryKeys.onlineOrders.all],
        },
    })
}
```
Verify `queryKeys.sales.all` and `queryKeys.stock.all` exist (recon confirms sales at keys.ts:61-72, stock present). If a name differs, use the actual one and report.

- [ ] **Step 2: Publication hooks**

`hooks/use-ecommerce-publication.ts`:
```typescript
"use client"

import { useMutation, useQuery } from "@tanstack/react-query"
import { ecommerceQueries } from "@/lib/query/options"
import { queryKeys } from "@/lib/query/keys"
import { ecommerceAPI } from "@/lib/api/ecommerce"
import type { PublicationUpdate } from "@/lib/types/ecommerce-publication"

export function useEcommercePublication() {
    return useQuery(ecommerceQueries.publication())
}

export function useSetEcommercePublication() {
    return useMutation({
        mutationFn: (update: PublicationUpdate) => ecommerceAPI.setPublication(update),
        meta: {
            successMessage: "Storefront catalog updated",
            invalidates: [queryKeys.ecommerce.all],
        },
    })
}
```

- [ ] **Step 3: Verify** (tsc; no new errors in the 2 files).
- [ ] **Step 4: Commit** — `feat(ecommerce-ui): query/mutation hooks for online orders and publication`.

---

### Task 3: Online Orders list page + columns + status filter

**Files:**
- Create: `app/(dashboard)/ecommerce/online-orders/page.tsx`
- Create: `app/(dashboard)/ecommerce/online-orders/columns.tsx`

**Interfaces:**
- Consumes: `useOnlineOrders`, the shared `DataTable` (`@/app/(dashboard)/inventory/products/data-table`), `TopBar`, `formatCurrency`, `formatDateTime`, `Badge`.
- Produces: a paged, status-filterable list at `/ecommerce/online-orders`; row click / action → `/ecommerce/online-orders/{id}`.

- [ ] **Step 1: Columns** (mirror `sales/invoices/columns.tsx`: rowNumber, status Badge, actions dropdown)

`app/(dashboard)/ecommerce/online-orders/columns.tsx`:
```typescript
"use client"

import Link from "next/link"
import type { ColumnDef } from "@tanstack/react-table"
import { MoreHorizontal } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import {
    DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { formatCurrency } from "@/lib/formatters/currency"
import { formatDateTime } from "@/lib/formatters/date"
import type { OnlineOrderRow, OnlineOrderStatus } from "@/lib/types/online-order"

const STATUS_VARIANT: Record<OnlineOrderStatus, "secondary" | "outline" | "destructive"> = {
    Pending: "outline",
    Confirmed: "secondary",
    Cancelled: "destructive",
}

interface ColumnsProps {
    pageIndex: number
    pageSize: number
    onView: (id: number) => void
}

export function createColumns({ pageIndex, pageSize, onView }: ColumnsProps): ColumnDef<OnlineOrderRow>[] {
    return [
        {
            id: "rowNumber",
            header: "#",
            enableSorting: false,
            enableHiding: false,
            cell: ({ row, table }) => {
                const idx = table.getRowModel().rows.findIndex((r) => r.id === row.id)
                return <span className="text-muted-foreground tabular-nums">{(pageIndex - 1) * pageSize + idx + 1}</span>
            },
        },
        {
            accessorKey: "orderNumber",
            header: "Order",
            cell: ({ row }) => <span className="font-mono text-xs">{row.original.orderNumber}</span>,
        },
        { accessorKey: "customerName", header: "Customer" },
        { accessorKey: "phone", header: "Phone", cell: ({ row }) => <span className="font-mono text-xs">{row.original.phone}</span> },
        { accessorKey: "itemCount", header: "Items", cell: ({ row }) => <span className="tabular-nums">{row.original.itemCount}</span> },
        {
            accessorKey: "itemsTotal",
            header: "Total",
            cell: ({ row }) => <span className="tabular-nums">{formatCurrency(row.original.itemsTotal)}</span>,
        },
        {
            accessorKey: "status",
            header: "Status",
            cell: ({ row }) => (
                <Badge variant={STATUS_VARIANT[row.original.status]} className="h-5 text-xs font-medium">
                    {row.original.status}
                </Badge>
            ),
        },
        {
            accessorKey: "createdAt",
            header: "Placed",
            cell: ({ row }) => <span className="text-muted-foreground">{formatDateTime(row.original.createdAt)}</span>,
        },
        {
            id: "actions",
            enableHiding: false,
            cell: ({ row }) => (
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button variant="ghost" className="h-6 w-6 p-0"><MoreHorizontal className="h-4 w-4" /></Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                        <DropdownMenuLabel>Actions</DropdownMenuLabel>
                        <DropdownMenuItem onClick={() => onView(row.original.id)}>View details</DropdownMenuItem>
                        <DropdownMenuItem asChild>
                            <Link href={`/ecommerce/online-orders/${row.original.id}`}>Open</Link>
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
            ),
        },
    ]
}
```

- [ ] **Step 2: Page** (mirror `sales/invoices/page.tsx`: 1-based pagination state, status Select filter, TopBar, shared DataTable, router.push to detail)

`app/(dashboard)/ecommerce/online-orders/page.tsx`:
```typescript
"use client"

import { useMemo, useState } from "react"
import { useRouter } from "next/navigation"
import { DataTable } from "@/app/(dashboard)/inventory/products/data-table"
import { TopBar } from "@/components/layout/TopBar"
import {
    Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from "@/components/ui/select"
import { useOnlineOrders } from "@/hooks/use-online-orders"
import { useRadiusClass } from "@/hooks/use-radius-class"
import type { OnlineOrderStatus } from "@/lib/types/online-order"
import { createColumns } from "./columns"

const STATUS_OPTIONS: { value: string; label: string }[] = [
    { value: "all", label: "All statuses" },
    { value: "Pending", label: "Pending" },
    { value: "Confirmed", label: "Confirmed" },
    { value: "Cancelled", label: "Cancelled" },
]

export default function OnlineOrdersPage() {
    const router = useRouter()
    const radiusClass = useRadiusClass()
    const [pageIndex, setPageIndex] = useState(1)
    const [pageSize, setPageSize] = useState(20)
    const [status, setStatus] = useState<string>("all")

    const filters = useMemo(
        () => ({ pageIndex, pageSize, status: status === "all" ? null : (status as OnlineOrderStatus) }),
        [pageIndex, pageSize, status],
    )
    const { data, isPending } = useOnlineOrders(filters)
    const rows = data?.data ?? []
    const total = data?.total ?? 0
    const pageCount = Math.ceil(total / pageSize) || 1

    const columns = useMemo(
        () => createColumns({ pageIndex, pageSize, onView: (id) => router.push(`/ecommerce/online-orders/${id}`) }),
        [pageIndex, pageSize, router],
    )

    return (
        <div className="space-y-3">
            <TopBar
                title="Online Orders"
                description="Guest orders placed on the storefront"
                filters={
                    <Select
                        value={status}
                        onValueChange={(v) => { setStatus(v); setPageIndex(1) }}
                    >
                        <SelectTrigger className="h-8 w-40 text-sm"><SelectValue /></SelectTrigger>
                        <SelectContent>
                            {STATUS_OPTIONS.map((o) => <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>)}
                        </SelectContent>
                    </Select>
                }
            />
            <div className={`border border-border/50 bg-card overflow-x-auto ${radiusClass}`}>
                <DataTable
                    columns={columns}
                    data={rows}
                    total={total}
                    pageCount={pageCount}
                    pageIndex={pageIndex}
                    pageSize={pageSize}
                    onPageChange={setPageIndex}
                    onPageSizeChange={(s) => { setPageSize(s); setPageIndex(1) }}
                    onRowDoubleClick={(row) => router.push(`/ecommerce/online-orders/${row.id}`)}
                />
            </div>
        </div>
    )
}
```
IMPORTANT: verify the shared `DataTable`'s actual prop names and the `TopBar` prop shape against the recon (DataTable props: columns, data, pageCount, pageIndex 1-based, pageSize, onPageChange, onPageSizeChange, onRowDoubleClick, total; TopBar has title/description/search/actions/filters/columnVisibility). If `isPending` should gate a `<Loader/>` like sales does, add it. Adapt names to the real components and report any deviation.

- [ ] **Step 3: Verify** (tsc; no new errors in the 2 files). Optionally start the app and eyeball the page renders (empty table is fine when no orders / storefront off).
- [ ] **Step 4: Commit** — `feat(ecommerce-ui): online orders list page`.

---

### Task 4: Online Order detail page + Confirm/Cancel actions

**Files:**
- Create: `app/(dashboard)/ecommerce/online-orders/[id]/page.tsx`
- Create: `app/(dashboard)/ecommerce/online-orders/[id]/_components/cancel-order-dialog.tsx`

**Interfaces:**
- Consumes: `useOnlineOrder`, `useConfirmOnlineOrder`, `useCancelOnlineOrder`, `ConfirmModal`, `formatCurrency`, `formatDateTimeWithTime`, `Badge`, `toast`.
- Produces: a detail route showing customer/address/items/totals + status; Confirm button (Pending only, ConfirmModal) and Cancel button (Pending only, reason dialog).

- [ ] **Step 1: Cancel dialog** (reason required; maps 422 or surfaces error)

`app/(dashboard)/ecommerce/online-orders/[id]/_components/cancel-order-dialog.tsx`:
```typescript
"use client"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import {
    Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle,
} from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"

interface Props {
    open: boolean
    onOpenChange: (open: boolean) => void
    onConfirm: (reason: string) => void
    isLoading: boolean
}

export function CancelOrderDialog({ open, onOpenChange, onConfirm, isLoading }: Props) {
    const [reason, setReason] = useState("")
    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader><DialogTitle>Cancel online order</DialogTitle></DialogHeader>
                <div className="space-y-2">
                    <Label htmlFor="cancel-reason" className="text-xs font-medium">Reason *</Label>
                    <Textarea id="cancel-reason" value={reason} onChange={(e) => setReason(e.target.value)}
                        placeholder="Why is this order being cancelled?" rows={3} />
                </div>
                <DialogFooter>
                    <Button variant="outline" onClick={() => onOpenChange(false)} disabled={isLoading}>Back</Button>
                    <Button variant="destructive" disabled={isLoading || reason.trim().length === 0}
                        onClick={() => onConfirm(reason.trim())}>
                        {isLoading ? "Cancelling..." : "Cancel order"}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    )
}
```

- [ ] **Step 2: Detail page** (mirror `sales/invoices/[id]/page.tsx`: `use()` params, skeleton, not-found, header/back)

`app/(dashboard)/ecommerce/online-orders/[id]/page.tsx`:
```typescript
"use client"

import { use, useState } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { ChevronLeft } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { ConfirmModal } from "@/components/ConfirmModal"
import { toast } from "sonner"
import { useOnlineOrder, useConfirmOnlineOrder, useCancelOnlineOrder } from "@/hooks/use-online-orders"
import { applyValidationErrors } from "@/lib/query/rhf" // only if a form is used; otherwise omit
import { formatCurrency } from "@/lib/formatters/currency"
import { formatDateTimeWithTime } from "@/lib/formatters/date"
import type { OnlineOrderStatus } from "@/lib/types/online-order"
import { CancelOrderDialog } from "./_components/cancel-order-dialog"

const STATUS_VARIANT: Record<OnlineOrderStatus, "secondary" | "outline" | "destructive"> = {
    Pending: "outline", Confirmed: "secondary", Cancelled: "destructive",
}

export default function OnlineOrderDetailPage({ params }: { params: Promise<{ id: string }> }) {
    const { id } = use(params)
    const router = useRouter()
    const { data: order, isPending } = useOnlineOrder(id)
    const confirm = useConfirmOnlineOrder()
    const cancel = useCancelOnlineOrder()
    const [confirmOpen, setConfirmOpen] = useState(false)
    const [cancelOpen, setCancelOpen] = useState(false)

    if (isPending) {
        return <div className="p-6 text-sm text-muted-foreground">Loading order…</div>
    }
    if (!order) {
        return (
            <div className="p-6 space-y-3">
                <p className="text-sm text-muted-foreground">Order not found.</p>
                <Button asChild variant="outline"><Link href="/ecommerce/online-orders">Back to orders</Link></Button>
            </div>
        )
    }

    const itemsTotal = order.items.reduce((s, i) => s + i.lineTotal, 0)
    const grandTotal = itemsTotal + order.deliveryFee
    const isPendingStatus = order.status === "Pending"

    const onConfirm = () => {
        confirm.mutate(order.id, {
            onSuccess: () => { setConfirmOpen(false); router.refresh() },
            onError: (e: unknown) => {
                const msg = (e as { message?: string })?.message ?? "Could not confirm the order"
                toast.error(msg)
            },
        })
    }
    const onCancel = (reason: string) => {
        cancel.mutate({ id: order.id, reason }, {
            onSuccess: () => { setCancelOpen(false); router.refresh() },
            onError: (e: unknown) => toast.error((e as { message?: string })?.message ?? "Could not cancel the order"),
        })
    }

    return (
        <div className="space-y-4">
            <div className="flex items-center justify-between border-b border-border/40 pb-2">
                <div className="flex items-center gap-2.5">
                    <Button variant="ghost" size="icon" onClick={() => router.push("/ecommerce/online-orders")} className="h-8 w-8">
                        <ChevronLeft className="h-4 w-4" />
                    </Button>
                    <div>
                        <h1 className="text-lg font-semibold font-mono">{order.orderNumber}</h1>
                        <p className="text-xs text-muted-foreground">Placed {formatDateTimeWithTime(order.createdAt)}</p>
                    </div>
                    <Badge variant={STATUS_VARIANT[order.status]} className="h-5 text-xs">{order.status}</Badge>
                </div>
                {isPendingStatus && (
                    <div className="flex items-center gap-2">
                        <Button variant="outline" size="sm" onClick={() => setCancelOpen(true)} disabled={cancel.isPending}>Cancel</Button>
                        <Button size="sm" onClick={() => setConfirmOpen(true)} disabled={confirm.isPending}>Confirm → create sale</Button>
                    </div>
                )}
            </div>

            <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
                <Card className="lg:col-span-2 border-border/50 shadow-none">
                    <CardHeader className="py-2 px-3 border-b border-border/50"><CardTitle className="text-sm">Items</CardTitle></CardHeader>
                    <CardContent className="p-0">
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="border-b border-border/50 text-left text-xs text-muted-foreground">
                                    <th className="p-2 font-medium">Product</th>
                                    <th className="p-2 font-medium font-mono">SKU</th>
                                    <th className="p-2 font-medium text-right">Unit</th>
                                    <th className="p-2 font-medium text-right">Qty</th>
                                    <th className="p-2 font-medium text-right">Line</th>
                                </tr>
                            </thead>
                            <tbody>
                                {order.items.map((it, i) => (
                                    <tr key={i} className="border-b border-border/30">
                                        <td className="p-2">{it.productTitle}</td>
                                        <td className="p-2 font-mono text-xs">{it.sku}</td>
                                        <td className="p-2 text-right tabular-nums">{formatCurrency(it.unitPrice)}</td>
                                        <td className="p-2 text-right tabular-nums">{it.quantity}</td>
                                        <td className="p-2 text-right tabular-nums">{formatCurrency(it.lineTotal)}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                        <div className="space-y-1 p-3 text-sm">
                            <div className="flex justify-between"><span className="text-muted-foreground">Items</span><span className="tabular-nums">{formatCurrency(itemsTotal)}</span></div>
                            <div className="flex justify-between"><span className="text-muted-foreground">Delivery</span><span className="tabular-nums">{formatCurrency(order.deliveryFee)}</span></div>
                            <div className="flex justify-between font-semibold"><span>Total</span><span className="tabular-nums">{formatCurrency(grandTotal)}</span></div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="border-border/50 shadow-none">
                    <CardHeader className="py-2 px-3 border-b border-border/50"><CardTitle className="text-sm">Customer</CardTitle></CardHeader>
                    <CardContent className="p-3 space-y-2 text-sm">
                        <div><div className="text-xs text-muted-foreground">Name</div><div>{order.customerName}</div></div>
                        <div><div className="text-xs text-muted-foreground">Phone</div><div className="font-mono text-xs">{order.phone}</div></div>
                        <div><div className="text-xs text-muted-foreground">Address</div><div className="whitespace-pre-wrap">{order.address}</div></div>
                        {order.note && <div><div className="text-xs text-muted-foreground">Note</div><div>{order.note}</div></div>}
                        {order.status === "Cancelled" && order.cancelReason && (
                            <div><div className="text-xs text-muted-foreground">Cancel reason</div><div>{order.cancelReason}</div></div>
                        )}
                        {order.status === "Confirmed" && order.saleId && (
                            <div><div className="text-xs text-muted-foreground">Sale</div>
                                <Link className="text-primary underline underline-offset-2 text-xs" href={`/sales/invoices/${order.saleId}`}>View sale</Link>
                            </div>
                        )}
                    </CardContent>
                </Card>
            </div>

            <ConfirmModal
                open={confirmOpen}
                onOpenChange={setConfirmOpen}
                title="Confirm this order?"
                description="A sale will be created and stock will be deducted for the storefront's selling branch."
                confirmLabel="Confirm"
                onConfirm={onConfirm}
                isLoading={confirm.isPending}
            />
            <CancelOrderDialog open={cancelOpen} onOpenChange={setCancelOpen} onConfirm={onCancel} isLoading={cancel.isPending} />
        </div>
    )
}
```
Remove the `applyValidationErrors` import if you don't use a RHF form here (the cancel dialog is plain state). Verify `ConfirmModal`'s exact prop names against `components/ConfirmModal.tsx` (recon: open, onOpenChange, title, description, onConfirm, confirmLabel, cancelLabel, isLoading) and the sale detail route path (`/sales/invoices/{id}`) exists. Confirm `formatDateTimeWithTime` is exported; if only `formatDateTime` exists, use it. Report adaptations.

- [ ] **Step 3: Verify** (tsc; no new errors in the 2 files).
- [ ] **Step 4: Commit** — `feat(ecommerce-ui): online order detail with confirm and cancel`.

---

### Task 5: Publication manager page (`/settings/ecommerce`)

**Files:**
- Create: `app/(dashboard)/settings/ecommerce/page.tsx`
- Create: `app/(dashboard)/settings/ecommerce/_components/publication-manager.tsx`

**Interfaces:**
- Consumes: `useEcommercePublication`, `useSetEcommercePublication`, `useRequirePermission`, `Checkbox`, `Input` (search), `Button`, `Card`, `toast`.
- Produces: `/settings/ecommerce` — a category list with a publish checkbox each; expanding a category shows its products each with a publish checkbox; a search box; "Publish all in category"; a single Save that diffs current vs. original and sends one `PublicationUpdate`.

- [ ] **Step 1: Publication manager component** (draft state + JSON-diff dirty tracking, matching the features/customization pattern)

`app/(dashboard)/settings/ecommerce/_components/publication-manager.tsx`:
```typescript
"use client"

import { useEffect, useMemo, useState } from "react"
import { ChevronDown, ChevronRight, Search } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Checkbox } from "@/components/ui/checkbox"
import { Input } from "@/components/ui/input"
import { useEcommercePublication, useSetEcommercePublication } from "@/hooks/use-ecommerce-publication"
import type { PublicationCategory } from "@/lib/types/ecommerce-publication"

type CatFlags = Record<number, boolean>
type ProdFlags = Record<number, boolean>

function buildFlags(cats: PublicationCategory[]): { cat: CatFlags; prod: ProdFlags } {
    const cat: CatFlags = {}
    const prod: ProdFlags = {}
    for (const c of cats) {
        cat[c.id] = c.isPublishedOnline
        for (const p of c.products) prod[p.id] = p.isPublishedOnline
    }
    return { cat, prod }
}

export function PublicationManager() {
    const { data, isPending } = useEcommercePublication()
    const save = useSetEcommercePublication()
    const cats = useMemo(() => data ?? [], [data])

    const [catFlags, setCatFlags] = useState<CatFlags>({})
    const [prodFlags, setProdFlags] = useState<ProdFlags>({})
    const [original, setOriginal] = useState<{ cat: CatFlags; prod: ProdFlags }>({ cat: {}, prod: {} })
    const [expanded, setExpanded] = useState<Set<number>>(new Set())
    const [search, setSearch] = useState("")

    useEffect(() => {
        const flags = buildFlags(cats)
        setCatFlags(flags.cat)
        setProdFlags(flags.prod)
        setOriginal(flags)
    }, [cats])

    const dirty = useMemo(
        () => JSON.stringify({ catFlags, prodFlags }) !== JSON.stringify({ catFlags: original.cat, prodFlags: original.prod }),
        [catFlags, prodFlags, original],
    )

    const filtered = useMemo(() => {
        const q = search.trim().toLowerCase()
        if (!q) return cats
        return cats
            .map((c) => ({
                ...c,
                products: c.products.filter((p) => p.title.toLowerCase().includes(q) || p.code.toLowerCase().includes(q)),
            }))
            .filter((c) => c.title.toLowerCase().includes(q) || c.products.length > 0)
    }, [cats, search])

    const toggleExpand = (id: number) =>
        setExpanded((prev) => {
            const next = new Set(prev)
            next.has(id) ? next.delete(id) : next.add(id)
            return next
        })

    const setCat = (id: number, v: boolean) => setCatFlags((f) => ({ ...f, [id]: v }))
    const setProd = (id: number, v: boolean) => setProdFlags((f) => ({ ...f, [id]: v }))
    const publishAll = (c: PublicationCategory, v: boolean) =>
        setProdFlags((f) => {
            const next = { ...f }
            for (const p of c.products) next[p.id] = v
            return next
        })

    const onSave = () => {
        const update = {
            publishCategoryIds: [] as number[], unpublishCategoryIds: [] as number[],
            publishProductIds: [] as number[], unpublishProductIds: [] as number[],
        }
        for (const id of Object.keys(catFlags).map(Number)) {
            if (catFlags[id] !== original.cat[id]) (catFlags[id] ? update.publishCategoryIds : update.unpublishCategoryIds).push(id)
        }
        for (const id of Object.keys(prodFlags).map(Number)) {
            if (prodFlags[id] !== original.prod[id]) (prodFlags[id] ? update.publishProductIds : update.unpublishProductIds).push(id)
        }
        save.mutate(update)
    }

    if (isPending) return <p className="text-sm text-muted-foreground">Loading catalog…</p>

    return (
        <div className="space-y-3">
            <div className="flex items-center justify-between gap-3">
                <div className="relative w-64">
                    <Search className="absolute left-2 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-muted-foreground" />
                    <Input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search products…" className="h-8 pl-7 text-sm" />
                </div>
                <Button size="sm" disabled={!dirty || save.isPending} onClick={onSave}>
                    {save.isPending ? "Saving…" : "Save changes"}
                </Button>
            </div>

            <div className="space-y-2">
                {filtered.map((c) => (
                    <Card key={c.id} className="border-border/50 shadow-none">
                        <CardHeader className="flex flex-row items-center justify-between gap-2 py-2 px-3">
                            <div className="flex items-center gap-2">
                                <Button variant="ghost" size="icon" className="h-6 w-6" onClick={() => toggleExpand(c.id)}>
                                    {expanded.has(c.id) ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
                                </Button>
                                <label className="flex items-center gap-2 cursor-pointer">
                                    <Checkbox checked={catFlags[c.id] ?? false} onCheckedChange={(v) => setCat(c.id, v === true)} />
                                    <CardTitle className="text-sm">{c.title}</CardTitle>
                                </label>
                                <span className="text-xs text-muted-foreground tabular-nums">{c.products.length} products</span>
                            </div>
                            <Button variant="outline" size="sm" className="h-6 text-xs" onClick={() => publishAll(c, true)}>Publish all</Button>
                        </CardHeader>
                        {expanded.has(c.id) && (
                            <CardContent className="p-3 pt-0 space-y-1">
                                {c.products.map((p) => (
                                    <label key={p.id} className="flex items-center gap-2 py-1 cursor-pointer">
                                        <Checkbox checked={prodFlags[p.id] ?? false} onCheckedChange={(v) => setProd(p.id, v === true)} />
                                        <span className="text-sm">{p.title}</span>
                                        <span className="font-mono text-xs text-muted-foreground">{p.code}</span>
                                    </label>
                                ))}
                                {c.products.length === 0 && <p className="text-xs text-muted-foreground">No products in this category.</p>}
                            </CardContent>
                        )}
                    </Card>
                ))}
                {filtered.length === 0 && <p className="text-sm text-muted-foreground">No categories match your search.</p>}
            </div>
        </div>
    )
}
```
Verify `Checkbox`'s `onCheckedChange` value type (shadcn passes `boolean | "indeterminate"` — the `v === true` guard handles it). Confirm `Checkbox` is installed (recon: yes).

- [ ] **Step 2: Page** (admin permission gate like `settings/features/page.tsx`)

`app/(dashboard)/settings/ecommerce/page.tsx`:
```typescript
"use client"

import { useRequirePermission } from "@/hooks/use-require-permission"
import { PublicationManager } from "./_components/publication-manager"

export default function EcommerceSettingsPage() {
    const allowed = useRequirePermission("Settings.System.Manage")
    if (!allowed) return null

    return (
        <div className="space-y-4 p-1">
            <div>
                <h1 className="text-lg font-semibold">Ecommerce catalog</h1>
                <p className="text-sm text-muted-foreground">
                    Choose which categories and products appear on the public storefront. Store name, hero, and delivery
                    fee are configured under Settings → Features → Ecommerce.
                </p>
            </div>
            <PublicationManager />
        </div>
    )
}
```
Verify `useRequirePermission`'s exact return contract (recon: `useRequirePermission(key, redirectTo="/dashboard")` — check whether it returns a boolean or just redirects; adapt the `if (!allowed) return null` guard to its real shape, mirroring `settings/features/page.tsx:17` exactly).

- [ ] **Step 3: Verify** (tsc; no new errors in the 2 files).
- [ ] **Step 4: Commit** — `feat(ecommerce-ui): storefront catalog publication manager`.

---

### Task 6: Notification deep-link + settings-features icon

**Files:**
- Modify: `components/layout/NotificationDropdown.tsx` (add `OnlineOrder` case to `buildDeepLink`)
- Modify: `components/layout/NotificationsAllModal.tsx` (same switch + optional TYPE_FILTERS entry)
- Modify: `app/(dashboard)/settings/features/_components/FeaturesSection.tsx` (optional: add `ecommerce` icon to `MODULE_ICONS`)

**Interfaces:**
- Consumes: existing notification deep-link switch; `OnlineOrderPlaced` notifications carry `relatedEntityType: "OnlineOrder"`, `relatedEntityId: orderNumber` (a string like `W000001`, NOT the numeric id).
- Produces: clicking an online-order notification navigates to the orders list (filtered), since the detail route needs the numeric id which the notification doesn't carry.

- [ ] **Step 1: Deep-link** — in BOTH `NotificationDropdown.tsx` and `NotificationsAllModal.tsx`, add a case to the `buildDeepLink` switch. Because `relatedEntityId` is the order NUMBER (not the numeric route id), link to the list page rather than a detail route:
```typescript
        case "OnlineOrder":
            return "/ecommerce/online-orders"
```
(Read each file's switch first — recon says it's duplicated at NotificationDropdown.tsx:30-47 and NotificationsAllModal.tsx:51-67; match the exact case-key the switch uses — it keys on `relatedEntityType`, so `"OnlineOrder"` is correct.)

- [ ] **Step 2: TYPE_FILTERS (optional, NotificationsAllModal.tsx:42-49)** — add `{ value: "OnlineOrder", label: "Online orders" }` so staff can filter the notification list to online orders (server does prefix match, so it catches `OnlineOrderPlaced`).

- [ ] **Step 3: Settings icon (optional, FeaturesSection.tsx:28-33)** — add `ecommerce: ShoppingCart` (import `ShoppingCart` from `lucide-react`) to `MODULE_ICONS` so the auto-rendered Ecommerce settings tab gets a proper icon instead of the generic fallback.

- [ ] **Step 4: Verify** (tsc; no new errors in the 3 files).
- [ ] **Step 5: Commit** — `feat(ecommerce-ui): notification deep-link and settings icon for ecommerce`.

---

### Task 7: Navigation module seed + final verification + push

**Files:**
- Create: `docs/ecommerce-nav-modules.json` (a bulk-module payload documenting the two nav entries)

**Context:** The sidebar is DB-driven (Module rows). Two entries are needed: a top-level **Ecommerce** module (`/ecommerce`, type 1) with an **Online Orders** child (`/ecommerce/online-orders`, type 2), and a **Ecommerce** settings child (`/settings/ecommerce`, type 2) under the existing Settings module. These are created at runtime via the `/settings/modules` admin UI or `POST /api/Module/bulk` — NOT frontend code. This task documents the exact payload and records the manual step; it does not require the API to be running.

- [ ] **Step 1: Document the nav payload**

`docs/ecommerce-nav-modules.json`:
```json
{
  "modules": [
    {
      "title": "Ecommerce",
      "icon": "ShoppingCart",
      "url": "/ecommerce",
      "type": 1,
      "order": 55,
      "isActive": true,
      "children": [
        {
          "title": "Online Orders",
          "icon": "ClipboardList",
          "url": "/ecommerce/online-orders",
          "type": 2,
          "order": 1,
          "isActive": true
        }
      ]
    }
  ]
}
```
Note in the file header comment (or the report) that the **Settings → Ecommerce** entry must be added separately as a `type: 2` child whose `parentId` is the existing Settings module id (resolve via `GET /api/Module` or the `/settings/modules` list — the id differs per DB), url `/settings/ecommerce`, icon `ShoppingCart`. It can't be pre-seeded with a static parentId.

- [ ] **Step 2: Full frontend verification**

Run: `cd C:\Personal\nexterp\NextErp_React && node_modules\.bin\tsc.cmd -p tsconfig.json --noEmit 2>&1 | Select-String "error TS"`.
Expected: the SAME baseline error set from Task 1 Step 1 — zero new errors in any file created/modified across Tasks 1–6. Diff the file-path set against the baseline and confirm no additions.

- [ ] **Step 3: Manual smoke (requires API + dev server running)**

With the API up (storefront can stay disabled) and `npm run dev` on `127.0.0.1:3000`, logged in as admin:
1. Seed the nav modules (via `/settings/modules/create` or `POST /api/Module/bulk` with the payload above), reload.
2. Visit `/ecommerce/online-orders` — table renders (empty is fine).
3. Visit `/settings/ecommerce` — categories/products load; toggle a product, Save → success toast; reload and confirm the flag persisted (`GET /api/Ecommerce/publication`).
4. Visit `/settings/features` — confirm an **Ecommerce** tab auto-appears with the store-config fields.
Document what you observed. If the API isn't runnable in this environment, record that the smoke is deferred and the tsc gate + code review stand in.

- [ ] **Step 4: Commit + push**
```powershell
git add docs/ecommerce-nav-modules.json
git commit -m "docs(ecommerce-ui): nav module seed payload for storefront admin

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
git push origin main
```

---

## Plan self-review notes (applied)

- **Spec coverage:** publication manager (owner's explicit "select/unselect category and products") = Task 5 ✓; online orders review/confirm/cancel = Tasks 3–4 ✓; store-config settings = auto-rendered at /settings/features, icon polish in Task 6 ✓; nav = Task 7 (DB rows, documented) ✓; notification surfacing = Task 6 ✓; server-state 3-layer = Tasks 1–2 ✓.
- **Type consistency:** `OnlineOrderStatus` union reused across columns/detail; `PublicationUpdate` shape matches the backend command; camelCase assumed throughout.
- **Verification reality:** frontend has pre-existing TS errors, so every task gates on "no NEW errors in touched files" against a baseline captured in Task 1, not a clean tsc — this is explicit and consistent with how Plan 1's frontend-adjacent checks worked.
- **Known execution checkpoints (verify, don't assume):** shared `DataTable` + `TopBar` exact props (Task 3); `ConfirmModal` props and `formatDateTimeWithTime` export (Task 4); `useRequirePermission` return contract (Task 5); the notification switch's exact case key and file line ranges (Task 6); `fetchAPI` 204/void handling (Task 1 Step 3).
- **Out of scope (Plan 3):** the public storefront itself; live pending-order count badge in nav (no existing pattern — deferred); the unpublished-parent/published-subcategory hierarchy nuance carried over from Plan 1's final review.
