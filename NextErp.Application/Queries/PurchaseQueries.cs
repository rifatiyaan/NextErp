using MediatR;
using NextErp.Application.DTOs;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Queries
{
    // Get by Id
    public record GetPurchaseByIdQuery(Guid Id) : IRequest<Entities.Purchase?>;

    // Get paged list
    public record GetPagedPurchasesQuery(
        int PageIndex,
        int PageSize,
        string? SearchText,
        string? SortBy
    ) : IRequest<(IList<Entities.Purchase> Records, int Total, int TotalDisplay)>;

    // Get purchase report
    public record GetPurchaseReportQuery(
        DateTime StartDate,
        DateTime EndDate,
        int? SupplierId
    ) : IRequest<Purchase.Response.Get.Report>;
}
