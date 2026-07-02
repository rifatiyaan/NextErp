using NextErp.Application.Common;
using MediatR;
using NextErp.Application.DTOs.Product;

namespace NextErp.Application.Queries
{
    // Get Product by Id
    public record GetProductByIdQuery(int Id) : IRequest<ProductResponse?>;

    // Next auto-generated product code (preview for the create form, e.g. "P000001")
    public record GetNextProductCodeQuery() : IRequest<string>;

    // Get Paged Products
    public record GetPagedProductsQuery(
        int PageIndex = 1,
        int PageSize = 10,
        string? SearchText = null,
        string? SortBy = null,
        int? CategoryId = null,
        string? Status = null,
        bool IncludeStock = false
    ) : IRequest<PagedResult<ProductResponse>>;
}
