using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.PurchaseReturn;
using NextErp.Application.DTOs.Returns;
using NextErp.Application.Queries.PurchaseReturn;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PurchaseReturnController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetPagedPurchaseReturnsQuery(pageIndex, pageSize, searchText), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetPurchaseReturnByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseReturnRequest dto,
        CancellationToken ct = default)
    {
        var id = await mediator.Send(new CreatePurchaseReturnCommand(dto), ct);
        var created = await mediator.Send(new GetPurchaseReturnByIdQuery(id), ct);
        return CreatedAtAction(nameof(Get), new { id }, created);
    }
}
