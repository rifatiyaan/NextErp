using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.PurchaseReturn;
using NextErp.Application.DTOs.Returns;

namespace NextErp.Application.Handlers.QueryHandlers.PurchaseReturn;

public sealed class GetPurchaseReturnByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetPurchaseReturnByIdQuery, PurchaseReturnResponse?>
{
    public async Task<PurchaseReturnResponse?> Handle(
        GetPurchaseReturnByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        return await db.PurchaseReturns
            .AsNoTracking()
            .Where(r => r.Id == request.Id)
            .Select(r => new PurchaseReturnResponse
            {
                Id = r.Id,
                ReturnNumber = r.ReturnNumber,
                PurchaseId = r.PurchaseId,
                PurchaseNumber = r.Purchase.PurchaseNumber,
                SupplierName = r.Purchase.Party != null ? r.Purchase.Party.Title : null,
                ReturnDate = r.ReturnDate,
                Reason = r.Reason,
                Notes = r.Notes,
                TotalAmount = r.TotalAmount,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                Items = r.Items.Select(i => new PurchaseReturnLineResponse
                {
                    Id = i.Id,
                    PurchaseItemId = i.PurchaseItemId,
                    ProductVariantId = i.ProductVariantId,
                    ProductTitle = i.ProductVariant.Product != null
                        ? i.ProductVariant.Product.Title
                        : "Product",
                    VariantSku = i.ProductVariant.Sku,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Quantity * i.UnitPrice,
                    ConditionNote = i.ConditionNote,
                }).ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed class GetPagedPurchaseReturnsHandler(IApplicationDbContext db)
    : IRequestHandler<GetPagedPurchaseReturnsQuery, PagedPurchaseReturnResponse>
{
    public async Task<PagedPurchaseReturnResponse> Handle(
        GetPagedPurchaseReturnsQuery request,
        CancellationToken cancellationToken = default)
    {
        var page = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var baseQuery = db.PurchaseReturns.AsNoTracking().Where(r => r.IsActive);
        var total = await baseQuery.CountAsync(cancellationToken);

        var filtered = baseQuery;
        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var s = request.SearchText.Trim();
            filtered = filtered.Where(r =>
                r.ReturnNumber.Contains(s) ||
                r.Purchase.PurchaseNumber.Contains(s) ||
                (r.Purchase.Party != null && r.Purchase.Party.Title.Contains(s)));
        }
        var totalDisplay = await filtered.CountAsync(cancellationToken);

        var rows = await filtered
            .OrderByDescending(r => r.ReturnDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new PurchaseReturnListRowResponse
            {
                Id = r.Id,
                ReturnNumber = r.ReturnNumber,
                PurchaseId = r.PurchaseId,
                PurchaseNumber = r.Purchase.PurchaseNumber,
                SupplierName = r.Purchase.Party != null ? r.Purchase.Party.Title : null,
                ReturnDate = r.ReturnDate,
                TotalAmount = r.TotalAmount,
                ItemCount = r.Items.Count,
            })
            .ToListAsync(cancellationToken);

        return new PagedPurchaseReturnResponse
        {
            Total = total,
            TotalDisplay = totalDisplay,
            Data = rows,
        };
    }
}
