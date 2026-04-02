using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using NextErp.Application.Queries;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SaleController(IMediator mediator, IMapper mapper) : ControllerBase
{
    // GET api/sale/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var query = new GetSaleByIdQuery(id);
        var sale = await mediator.Send(query);

        if (sale == null) return NotFound();

        var dto = mapper.Map<Sale.Response.Get.Single>(sale);
        return Ok(dto);
    }

    // GET api/sale  (supports page + pageSize or pageIndex + pageSize)
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int? page = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] string? sortBy = null)
    {
        var effectivePage = page ?? pageIndex ?? 1;
        if (effectivePage < 1) effectivePage = 1;
        if (pageSize < 1) pageSize = 10;

        var query = new GetPagedSalesQuery(effectivePage, pageSize, searchText, sortBy);
        var pagedResult = await mediator.Send(query);

        return Ok(new
        {
            total = pagedResult.Total,
            totalDisplay = pagedResult.TotalDisplay,
            page = effectivePage,
            pageSize,
            data = pagedResult.Records
        });
    }

    // POST api/sale
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Sale.Request.Create.Single dto)
    {
        try
        {
            var command = new CreateSaleCommand(
                dto.PartyId,
                dto.Discount,
                dto.PaymentMethod,
                dto.PaidAmount,
                dto.Items
            );

            var id = await mediator.Send(command);

            var sale = await mediator.Send(new GetSaleByIdQuery(id));
            var response = mapper.Map<Sale.Response.Get.Single>(sale);

            return CreatedAtAction(nameof(Get), new { id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET api/sale/report
    [HttpGet("report")]
    public async Task<IActionResult> GetReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? partyId = null)
    {
        var query = new GetSalesReportQuery(startDate, endDate, partyId);
        var report = await mediator.Send(query);

        return Ok(report);
    }
}
