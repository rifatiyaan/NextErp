using MediatR;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Queries
{
    // Get by Id
    public record GetCustomerByIdQuery(Guid Id) : IRequest<Entities.Customer?>;

    // Get paged list
    public record GetPagedCustomersQuery(
        int PageIndex,
        int PageSize,
        string? SearchText,
        string? SortBy
    ) : IRequest<(IList<Entities.Customer> Records, int Total, int TotalDisplay)>;
}
