using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Stock;
using NextErp.Application.DTOs;
using NextErp.Application.Queries;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class StockController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet("variant/{productVariantId:int}")]
    public async Task<IActionResult> GetByProductVariantId(int productVariantId)
    {
        var stock = await mediator.Send(new GetStockByProductVariantIdQuery(productVariantId));

        if (stock == null)
            return NotFound();

        return Ok(mapper.Map<Stock.Response.Single>(stock));
    }

    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetByProductId(int productId)
    {
        var stocks = await mediator.Send(new GetStocksByProductIdQuery(productId));
        return Ok(mapper.Map<List<Stock.Response.Single>>(stocks));
    }

    [HttpGet("report/current")]
    public async Task<IActionResult> GetCurrentStockReport()
    {
        var report = await mediator.Send(new GetCurrentStockReportQuery());
        return Ok(report);
    }

    [HttpGet("report/low")]
    public async Task<IActionResult> GetLowStockReport()
    {
        var report = await mediator.Send(new GetLowStockReportQuery());
        return Ok(report);
    }

    [HttpPatch("variant/{productVariantId:int}/reorder-level")]
    public async Task<IActionResult> SetReorderLevel(int productVariantId, [FromBody] decimal? reorderLevel)
    {
        await mediator.Send(new SetReorderLevelCommand(productVariantId, reorderLevel));
        return NoContent();
    }

    /// <summary>Append-only movement history for a variant in a branch (admin / inventory).</summary>
    [HttpGet("variant/{productVariantId:int}/movements")]
    public async Task<IActionResult> GetMovementHistory(
        int productVariantId,
        [FromQuery] Guid branchId)
    {
        if (branchId == Guid.Empty)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Bad request", detail: "branchId is required.");

        var rows = await mediator.Send(new GetStockMovementHistoryQuery(productVariantId, branchId));
        return Ok(rows);
    }
}
