using NextErp.Application.Common;
using MediatR;
using DTOs = NextErp.Application.DTOs;

namespace NextErp.Application.Queries
{
    // Get Product by Id
    public record GetProductByIdQuery(int Id) : IRequest<DTOs.Product.Response.Get.Single?>;

    // Get Paged Products
    public record GetPagedProductsQuery(
        int PageIndex = 1,
        int PageSize = 10,
        string? SearchText = null,
        string? SortBy = null,
        int? CategoryId = null,
        string? Status = null,
        bool IncludeStock = false
    ) : IRequest<PagedResult<DTOs.Product.Response.Get.Single>>;
}
