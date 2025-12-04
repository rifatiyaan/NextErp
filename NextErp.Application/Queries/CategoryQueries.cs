using NextErp.Application.Common; // <-- import PagedResult<T>
using NextErp.Domain.Entities;
using MediatR;

namespace NextErp.Application.Queries
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
