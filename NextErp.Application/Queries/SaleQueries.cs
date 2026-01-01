using MediatR;
using NextErp.Application.DTOs;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Queries
{
    // Get by Id
    public record GetSaleByIdQuery(Guid Id) : IRequest<Entities.Sale?>;

    // Get paged list
    public record GetPagedSalesQuery(
        int PageIndex,
        int PageSize,
        string? SearchText,
        string? SortBy
    ) : IRequest<(IList<Entities.Sale> Records, int Total, int TotalDisplay)>;

    // Get sales report
    public record GetSalesReportQuery(
        DateTime StartDate,
        DateTime EndDate,
        Guid? CustomerId
    ) : IRequest<Sale.Response.Get.Report>;
}
