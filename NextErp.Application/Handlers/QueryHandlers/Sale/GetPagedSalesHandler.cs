using NextErp.Application.Common;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SaleDto = NextErp.Application.DTOs.Sale;

namespace NextErp.Application.Handlers.QueryHandlers.Sale
{
    public class GetPagedSalesHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetPagedSalesQuery, PagedResult<SaleDto.Response.Get.ListRow>>
    {
        public async Task<PagedResult<SaleDto.Response.Get.ListRow>> Handle(
            GetPagedSalesQuery request,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = dbContext.Sales.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                baseQuery = baseQuery.Where(s =>
                    s.Title.Contains(term) ||
                    s.SaleNumber.Contains(term) ||
                    (s.Party != null && s.Party.Title.Contains(term)));
            }

            var total = await baseQuery.CountAsync(cancellationToken);

            baseQuery = request.SortBy?.ToLowerInvariant() switch
            {
                "saledate" => baseQuery.OrderByDescending(s => s.SaleDate),
                "finalamount" => baseQuery.OrderByDescending(s => s.FinalAmount),
                "salenumber" => baseQuery.OrderBy(s => s.SaleNumber),
                _ => baseQuery.OrderByDescending(s => s.CreatedAt),
            };

            var rows = await baseQuery
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new SaleDto.Response.Get.ListRow
                {
                    Id = s.Id,
                    SaleNumber = s.SaleNumber,
                    CustomerName = s.Party != null ? s.Party.Title : string.Empty,
                    SaleDate = s.SaleDate,
                    FinalAmount = s.FinalAmount,
                    TotalPaid = s.Payments.Sum(p => p.Amount),
                    BalanceDue = s.FinalAmount - s.Payments.Sum(p => p.Amount),
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<SaleDto.Response.Get.ListRow>(rows, total, total);
        }
    }
}
