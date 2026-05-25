using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Accounting;
using NextErp.Application.DTOs.Accounting;
using NextErp.Application.Queries.Accounting;
using NextErp.Domain.Entities;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AccountController(IMediator mediator) : ControllerBase
{
    // GET api/account?pageIndex=1&pageSize=50&type=Asset&onlyPostable=true
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchText = null,
        [FromQuery] AccountType? type = null,
        [FromQuery] bool? onlyPostable = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetPagedAccountsQuery(pageIndex, pageSize, searchText, type, onlyPostable), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetAccountByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AccountDto.Request.Create dto, CancellationToken ct = default)
    {
        var id = await mediator.Send(new CreateAccountCommand(dto), ct);
        var created = await mediator.Send(new GetAccountByIdQuery(id), ct);
        return CreatedAtAction(nameof(Get), new { id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AccountDto.Request.Update dto, CancellationToken ct = default)
    {
        var ok = await mediator.Send(new UpdateAccountCommand(id, dto), ct);
        if (!ok) return NotFound();
        var updated = await mediator.Send(new GetAccountByIdQuery(id), ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct = default)
    {
        var ok = await mediator.Send(new DeactivateAccountCommand(id), ct);
        if (!ok) return NotFound();
        return NoContent();
    }
}
