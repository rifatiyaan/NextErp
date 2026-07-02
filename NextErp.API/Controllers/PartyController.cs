using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands;
using NextErp.Application.DTOs.Common;
using NextErp.Application.DTOs.Party;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PartyController(
    IMediator mediator,
    IBackgroundJobClient backgroundJobs) : ControllerBase
{
    // GET api/party/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var party = await mediator.Send(new GetPartyByIdQuery(id));
        if (party == null) return NotFound();

        var dto = party.ToResponse();
        return Ok(dto);
    }

    // GET api/party?type=Customer&pageIndex=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] PartyType? type = null)
    {
        var query = new GetPagedPartiesQuery(pageIndex, pageSize, searchText, sortBy, type);
        var result = await mediator.Send(query);
        var dtoList = result.Records.Select(p => p.ToResponse()).ToList();

        return Ok(new
        {
            total = result.Total,
            totalDisplay = result.TotalDisplay,
            data = dtoList
        });
    }

    // POST api/party
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartyRequest dto)
    {
        var command = dto.ToCommand();
        var id = await mediator.Send(command);

        var party = await mediator.Send(new GetPartyByIdQuery(id));
        var response = party!.ToCreateResponse();

        return CreatedAtAction(nameof(Get), new { id }, response);
    }

    // PUT api/party/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePartyRequest dto)
    {
        dto.Id = id;
        var command = dto.ToCommand();
        await mediator.Send(command);
        return NoContent();
    }

    // POST api/party/customers/batch/deactivate
    // Filters by PartyType=Customer in the handler so only customers are touched.
    [HttpPost("customers/batch/deactivate")]
    public async Task<ActionResult<object>> BatchDeactivateCustomers(
        [FromBody] BatchIdsDto<Guid> dto,
        CancellationToken cancellationToken = default)
    {
        var count = await mediator.Send(
            new BatchDeactivateCustomersCommand(dto?.Ids ?? new List<Guid>()),
            cancellationToken);
        return Ok(new { deactivated = count });
    }

    // POST api/party/suppliers/batch/deactivate
    [HttpPost("suppliers/batch/deactivate")]
    public async Task<ActionResult<object>> BatchDeactivateSuppliers(
        [FromBody] BatchIdsDto<Guid> dto,
        CancellationToken cancellationToken = default)
    {
        var count = await mediator.Send(
            new BatchDeactivateSuppliersCommand(dto?.Ids ?? new List<Guid>()),
            cancellationToken);
        return Ok(new { deactivated = count });
    }

    // POST api/party/customers/bulk-email
    //
    // Body: { ids: [guid, …], subject: "...", body: "..." }
    // Returns: { queued, skippedNoEmail, skippedNotFound }
    //
    // The endpoint is intentionally fast (returns immediately after
    // partitioning). All actual SMTP sends happen on Hangfire workers.
    // We do the Enqueue here in the controller — the handler stays
    // Hangfire-agnostic for unit-test friendliness.
    [HttpPost("customers/bulk-email")]
    public async Task<ActionResult<object>> BulkEmailCustomers(
        [FromBody] BulkEmailCustomersRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var subject = dto?.Subject ?? string.Empty;
        var body = dto?.Body ?? string.Empty;

        var result = await mediator.Send(
            new BulkEmailCustomersCommand(
                dto?.Ids ?? new List<Guid>(),
                subject,
                body),
            cancellationToken);

        foreach (var customerId in result.QueueableCustomerIds)
        {
            backgroundJobs.Enqueue<ICustomerBroadcastEmailService>(svc =>
                svc.SendBroadcastAsync(customerId, subject, body, CancellationToken.None));
        }

        return Ok(new
        {
            queued = result.Queued,
            skippedNoEmail = result.SkippedNoEmail,
            skippedNotFound = result.SkippedNotFound,
        });
    }

    public sealed class BulkEmailCustomersRequestDto
    {
        public List<Guid> Ids { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    // DELETE api/party/{id} — soft delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var party = await mediator.Send(new GetPartyByIdQuery(id));
        if (party == null) return NotFound();

        party.IsActive = false;
        party.UpdatedAt = DateTime.UtcNow;

        var command = new UpdatePartyCommand(
            party.Id, party.Title, party.FirstName, party.LastName,
            party.Email, party.Phone, party.Address, party.ContactPerson,
            party.LoyaltyCode, party.NationalId, party.VatNumber, party.TaxId,
            party.Notes, party.PartyType, false);

        await mediator.Send(command);
        return NoContent();
    }
}
