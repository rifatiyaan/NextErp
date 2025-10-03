using MediatR;

namespace EcommerceApplicationWeb.Application.Features.Categories.Queries
{
    // Get By Id
    public record GetCategoryByIdQuery(int Id) : IRequest<Domain.Entities.Category?>;

    // Get Paged
    public record GetPagedCategoriesQuery(
        int PageIndex,
        int PageSize,
        string? SearchText,
        string? SortBy
    ) : IRequest<PagedResult<Domain.Entities.Category>>;

    // Optional: Create a PagedResult record for better type safety
    public record PagedResult<T>(IList<T> Records, int Total, int TotalDisplay);
}