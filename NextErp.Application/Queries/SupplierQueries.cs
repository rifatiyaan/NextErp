using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Queries
{
    // Get by Id
    public record GetSupplierByIdQuery(int Id) : IRequest<Entities.Supplier?>;

    // Get paged list
    public record GetPagedSuppliersQuery(
        int PageIndex,
        int PageSize,
        string? SearchText,
        string? SortBy
    ) : IRequest<(IList<Entities.Supplier> Records, int Total, int TotalDisplay)>;
}
