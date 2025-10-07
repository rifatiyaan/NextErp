namespace EcommerceApplicationWeb.Application.Common
{
    public record PagedResult<T>(IList<T> Records, int Total, int TotalDisplay);
}
