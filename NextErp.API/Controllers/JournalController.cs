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
public class JournalController(IMediator mediator) : ControllerBase
{
    // GET api/journal?pageIndex=1&pageSize=10&referenceType=Transfer
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] JournalEntryReferenceType? referenceType = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetPagedJournalEntriesQuery(pageIndex, pageSize, searchText, referenceType), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetJournalEntryByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // POST api/journal/transfer
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferJournalRequest dto, CancellationToken ct = default)
    {
        var id = await mediator.Send(new CreateAccountTransferCommand(dto), ct);
        var created = await mediator.Send(new GetJournalEntryByIdQuery(id), ct);
        return CreatedAtAction(nameof(Get), new { id }, created);
    }
}
