using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Promotion;
using NextErp.Application.DTOs.Promotion;
using NextErp.Application.Queries.Promotion;
using NextErp.Domain.Entities;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PromotionController(IMediator mediator) : ControllerBase
{
    // GET api/promotion?pageIndex=1&pageSize=50&type=Bogo&onlyActive=true
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchText = null,
        [FromQuery] PromotionType? type = null,
        [FromQuery] bool? onlyActive = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetPagedPromotionsQuery(pageIndex, pageSize, searchText, type, onlyActive), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetPromotionByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromotionRequest dto, CancellationToken ct = default)
    {
        var id = await mediator.Send(new CreatePromotionCommand(dto), ct);
        var created = await mediator.Send(new GetPromotionByIdQuery(id), ct);
        return CreatedAtAction(nameof(Get), new { id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromotionRequest dto, CancellationToken ct = default)
    {
        var ok = await mediator.Send(new UpdatePromotionCommand(id, dto), ct);
        if (!ok) return NotFound();
        var updated = await mediator.Send(new GetPromotionByIdQuery(id), ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct = default)
    {
        var ok = await mediator.Send(new DeactivatePromotionCommand(id), ct);
        if (!ok) return NotFound();
        return NoContent();
    }
}
