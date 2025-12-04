using NextErp.Application.Common; // <-- import PagedResult<T>
using NextErp.Domain.Entities;
using MediatR;

namespace NextErp.Application.Queries
{
    // Get Product by Id
    public record GetProductByIdQuery(int Id) : IRequest<Product?>;

    // Get Paged Products
    public record GetPagedProductsQuery(
        int PageIndex = 1,
        int PageSize = 10,
        string? SearchText = null,
        string? SortBy = null
    ) : IRequest<PagedResult<Product>>; // PagedResult is now imported
}
