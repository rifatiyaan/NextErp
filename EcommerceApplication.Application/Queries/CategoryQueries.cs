using EcommerceApplicationWeb.Application.Common; // <-- import PagedResult<T>
using EcommerceApplicationWeb.Domain.Entities;
using MediatR;

namespace EcommerceApplicationWeb.Application.Queries
{
    // Get By Id
    public record GetCategoryByIdQuery(int Id) : IRequest<Category?>;

    // Get Paged
    public record GetPagedCategoriesQuery(
        int PageIndex,
        int PageSize,
        string? SearchText,
        string? SortBy
    ) : IRequest<PagedResult<Category>>; // PagedResult is now imported
}
