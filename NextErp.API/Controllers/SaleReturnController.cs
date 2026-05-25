using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.SaleReturn;
using NextErp.Application.DTOs.Returns;
using NextErp.Application.Queries.SaleReturn;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SaleReturnController(IMediator mediator) : ControllerBase
{
    // GET api/saleReturn?pageIndex=1&pageSize=10&searchText=...
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetPagedSaleReturnsQuery(pageIndex, pageSize, searchText), ct);
        return Ok(result);
    }

    // GET api/saleReturn/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetSaleReturnByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // POST api/saleReturn
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] SaleReturnDto.Request.Create.Single dto,
        CancellationToken ct = default)
    {
        var id = await mediator.Send(new CreateSaleReturnCommand(dto), ct);
        var created = await mediator.Send(new GetSaleReturnByIdQuery(id), ct);
        return CreatedAtAction(nameof(Get), new { id }, created);
    }
}
