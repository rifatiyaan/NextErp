using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands;
using NextErp.Application.DTOs.Purchase;
using NextErp.Application.Mapping;
using NextErp.Application.Queries;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PurchaseController(IMediator mediator) : ControllerBase
{
    // GET api/purchase/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var query = new GetPurchaseByIdQuery(id);
        var purchase = await mediator.Send(query);

        if (purchase == null) return NotFound();

        var dto = purchase.ToResponse();
        return Ok(dto);
    }

    // GET api/purchase
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string[]? status = null)
    {
        var query = new GetPagedPurchasesQuery(
            pageIndex,
            pageSize,
            searchText,
            sortBy,
            null,
            ParsePurchaseStatusFilter(status));
        var pagedResult = await mediator.Send(query);

        var dtoList = pagedResult.Records.Select(p => p.ToResponse()).ToList();

        return Ok(new
        {
            total = pagedResult.Total,
            totalDisplay = pagedResult.TotalDisplay,
            data = dtoList
        });
    }

    // POST api/purchase
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseRequest dto)
    {
        var command = new CreatePurchaseCommand(
            dto.Title,
            dto.PurchaseNumber,
            dto.PartyId,
            dto.PurchaseDate,
            dto.Discount,
            dto.Items,
            dto.Metadata
        );

        var id = await mediator.Send(command);

        var purchase = await mediator.Send(new GetPurchaseByIdQuery(id));
        var response = purchase!.ToCreateResponse();

        return CreatedAtAction(nameof(Get), new { id }, response);
    }

    // GET api/purchase/report
    [HttpGet("report")]
    public async Task<IActionResult> GetReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? partyId = null)
    {
        var query = new GetPurchaseReportQuery(startDate, endDate, partyId);
        var report = await mediator.Send(query);

        return Ok(report);
    }

    private static bool? ParsePurchaseStatusFilter(string[]? status)
    {
        if (status == null || status.Length == 0)
            return null;

        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in status)
        {
            if (!string.IsNullOrWhiteSpace(s))
                set.Add(s.Trim().ToLowerInvariant());
        }

        var active = set.Contains("active");
        var inactive = set.Contains("inactive");
        if (active && inactive)
            return null;
        if (active)
            return true;
        if (inactive)
            return false;
        return null;
    }
}
