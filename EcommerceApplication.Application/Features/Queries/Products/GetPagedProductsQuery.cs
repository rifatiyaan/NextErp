using EcommerceApplicationWeb.Application.Features.Categories.Queries;
using EcommerceApplicationWeb.Domain.Entities;
using MediatR;

namespace EcommerceApplicationWeb.Application.Features.Products.Queries
{
    public class GetPagedProductsQuery : IRequest<PagedResult<Product>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchText { get; set; }
        public string? SortBy { get; set; }

        public GetPagedProductsQuery(int pageIndex, int pageSize, string? searchText, string? sortBy)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            SearchText = searchText;
            SortBy = sortBy;
        }
    }
}
