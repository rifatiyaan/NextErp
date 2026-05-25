using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.SaleReturn;
using NextErp.Application.DTOs.Returns;

namespace NextErp.Application.Handlers.QueryHandlers.SaleReturn;

public sealed class GetSaleReturnByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetSaleReturnByIdQuery, SaleReturnDto.Response.Get.Single?>
{
    public async Task<SaleReturnDto.Response.Get.Single?> Handle(
        GetSaleReturnByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        return await db.SaleReturns
            .AsNoTracking()
            .Where(r => r.Id == request.Id)
            .Select(r => new SaleReturnDto.Response.Get.Single
            {
                Id = r.Id,
                ReturnNumber = r.ReturnNumber,
                SaleId = r.SaleId,
                SaleNumber = r.Sale.SaleNumber,
                CustomerName = r.Sale.Party != null ? r.Sale.Party.Title : null,
                ReturnDate = r.ReturnDate,
                Reason = r.Reason,
                Notes = r.Notes,
                TotalRefund = r.TotalRefund,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                Items = r.Items.Select(i => new SaleReturnDto.Response.Get.Line
                {
                    Id = i.Id,
                    SaleItemId = i.SaleItemId,
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

public sealed class GetPagedSaleReturnsHandler(IApplicationDbContext db)
    : IRequestHandler<GetPagedSaleReturnsQuery, SaleReturnDto.Response.Paged>
{
    public async Task<SaleReturnDto.Response.Paged> Handle(
        GetPagedSaleReturnsQuery request,
        CancellationToken cancellationToken = default)
    {
        var page = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var baseQuery = db.SaleReturns.AsNoTracking().Where(r => r.IsActive);
        var total = await baseQuery.CountAsync(cancellationToken);

        var filtered = baseQuery;
        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var s = request.SearchText.Trim();
            filtered = filtered.Where(r =>
                r.ReturnNumber.Contains(s) ||
                r.Sale.SaleNumber.Contains(s) ||
                (r.Sale.Party != null && r.Sale.Party.Title.Contains(s)));
        }
        var totalDisplay = await filtered.CountAsync(cancellationToken);

        var rows = await filtered
            .OrderByDescending(r => r.ReturnDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new SaleReturnDto.Response.Get.ListRow
            {
                Id = r.Id,
                ReturnNumber = r.ReturnNumber,
                SaleId = r.SaleId,
                SaleNumber = r.Sale.SaleNumber,
                CustomerName = r.Sale.Party != null ? r.Sale.Party.Title : null,
                ReturnDate = r.ReturnDate,
                TotalRefund = r.TotalRefund,
                ItemCount = r.Items.Count,
            })
            .ToListAsync(cancellationToken);

        return new SaleReturnDto.Response.Paged
        {
            Total = total,
            TotalDisplay = totalDisplay,
            Data = rows,
        };
    }
}
