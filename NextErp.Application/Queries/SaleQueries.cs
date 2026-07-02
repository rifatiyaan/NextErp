using MediatR;
using NextErp.Application.Common;
using Entities = NextErp.Domain.Entities;
using SaleDto = NextErp.Application.DTOs.Sale;

namespace NextErp.Application.Queries
{
    // Get by Id
    public record GetSaleByIdQuery(Guid Id) : IRequest<Entities.Sale?>;

    // Get paged list (projected; no line items or payment rows)
    public record GetPagedSalesQuery(
        int PageIndex,
        int PageSize,
        string? SearchText,
        string? SortBy
    ) : IRequest<PagedResult<SaleDto.SaleListRowResponse>>;

    // Get sales report
    public record GetSalesReportQuery(
        DateTime StartDate,
        DateTime EndDate,
        Guid? PartyId
    ) : IRequest<SaleDto.SaleReportResponse>;

    public record PreviewSalePricingQuery(
        List<SaleDto.PreviewLineRequest> Lines,
        Guid? PartyId
    ) : IRequest<SaleDto.PreviewSaleResponse>;
}
