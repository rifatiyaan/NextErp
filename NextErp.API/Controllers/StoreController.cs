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
        [FromQuery] int pageSize = 24)
    {
        var page = await mediator.Send(new GetStorePagedProductsQuery(categoryId, searchText, pageIndex, pageSize));
        return Ok(new { total = page.Total, data = page.Data });
    }

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

    [HttpGet("products/{id:int}/reviews")]
    public async Task<IActionResult> Reviews(int id) =>
        Ok(await mediator.Send(new GetProductReviewsQuery(id)));

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
