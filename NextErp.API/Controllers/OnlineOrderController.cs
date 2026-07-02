using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Queries.Ecommerce;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class OnlineOrderController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? status = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var page = await mediator.Send(new GetPagedOnlineOrdersQuery(status, pageIndex, pageSize));
        return Ok(new { total = page.Total, data = page.Data });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var detail = await mediator.Send(new GetOnlineOrderByIdQuery(id));
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        var saleId = await mediator.Send(new ConfirmOnlineOrderCommand(id));
        return Ok(new { saleId });
    }

    public sealed record CancelRequest(string Reason);

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelRequest body)
    {
        await mediator.Send(new CancelOnlineOrderCommand(id, body.Reason));
        return NoContent();
    }
}
