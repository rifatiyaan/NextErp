using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NextErp.API.Filters;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Queries.Ecommerce;

namespace NextErp.API.Controllers;

[AllowAnonymous]
[Route("api/store")]
[ApiController]
[EnableRateLimiting("store")]
[ServiceFilter(typeof(StorefrontEnabledFilter))]
public class StoreController(IMediator mediator) : ControllerBase
{
    [HttpGet("config")]
    public async Task<IActionResult> Config() =>
        Ok(await mediator.Send(new GetStoreConfigQuery()));

    [HttpGet("categories")]
    public async Task<IActionResult> Categories() =>
        Ok(await mediator.Send(new GetStoreCategoriesQuery()));

    [HttpGet("products")]
    public async Task<IActionResult> Products(
        [FromQuery] int? categoryId = null,
        [FromQuery] string? searchText = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 24,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? sort = null)
    {
        var page = await mediator.Send(new GetStorePagedProductsQuery(
            categoryId, searchText, pageIndex, pageSize, minPrice, maxPrice, sort));
        return Ok(new { total = page.Total, data = page.Data });
    }

    [HttpGet("price-range")]
    public async Task<IActionResult> PriceRange([FromQuery] int? categoryId = null) =>
        Ok(await mediator.Send(new GetStorePriceRangeQuery(categoryId)));

    [HttpGet("products/{id:int}")]
    public async Task<IActionResult> Product(int id)
    {
        var detail = await mediator.Send(new GetStoreProductByIdQuery(id));
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPost("orders")]
    [EnableRateLimiting("store-orders")]
    public async Task<IActionResult> CreateOrder([FromBody] StoreOrderCreateRequest request)
    {
        // Honeypot tripped: pretend success, store nothing.
        if (!string.IsNullOrEmpty(request.Website))
            return Ok(new { orderNumber = "W000000" });

        var orderNumber = await mediator.Send(new CreateOnlineOrderCommand(
            request.CustomerName, request.Phone, request.Address, request.Note, request.Items));
        return Ok(new { orderNumber });
    }

    // Public order tracking. Enumeration protection is the required phone match
    // (an attacker must guess a valid number AND its phone), not the rate limit —
    // so this idempotent GET uses the generous class-level "store" budget
    // (120/min/IP) rather than the 5/min "store-orders" bucket it would otherwise
    // share with checkout, which broke legitimate users behind a shared/CGNAT IP.
    [HttpGet("orders/{number}")]
    public async Task<IActionResult> OrderStatus(string number, [FromQuery] string? phone = null)
    {
        var status = await mediator.Send(new GetStoreOrderStatusQuery(number, phone ?? ""));
        return status is null ? NotFound() : Ok(status);
    }

    [HttpGet("products/{id:int}/reviews")]
    public async Task<IActionResult> Reviews(int id) =>
        Ok(await mediator.Send(new GetProductReviewsQuery(id)));

    [HttpGet("reviews/recent")]
    public async Task<IActionResult> RecentReviews([FromQuery] int take = 9) =>
        Ok(await mediator.Send(new GetRecentReviewsQuery(Math.Clamp(take, 1, 30))));

    [HttpPost("products/{id:int}/reviews")]
    [EnableRateLimiting("store-orders")]
    public async Task<IActionResult> CreateReview(int id, [FromBody] StoreReviewCreateRequest request)
    {
        // Honeypot tripped: pretend success, store nothing.
        if (!string.IsNullOrEmpty(request.Website))
            return Ok(new { id = 0 });

        var reviewId = await mediator.Send(new CreateReviewCommand(id, request.AuthorName, request.Rating, request.Text));
        return Ok(new { id = reviewId });
    }
}
