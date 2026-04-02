using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;
using PartyDto = NextErp.Application.DTOs.Party;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PartyController(IMediator mediator, IMapper mapper) : ControllerBase
{
    // GET api/party/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var party = await mediator.Send(new GetPartyByIdQuery(id));
        if (party == null) return NotFound();

        var dto = mapper.Map<PartyDto.Response.Get.Single>(party);
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
        var dtoList = mapper.Map<List<PartyDto.Response.Get.Single>>(result.Records);

        return Ok(new
        {
            total = result.Total,
            totalDisplay = result.TotalDisplay,
            data = dtoList
        });
    }

    // POST api/party
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PartyDto.Request.Create.Single dto)
    {
        var command = mapper.Map<CreatePartyCommand>(dto);
        var id = await mediator.Send(command);

        var party = await mediator.Send(new GetPartyByIdQuery(id));
        var response = mapper.Map<PartyDto.Response.Create.Single>(party);

        return CreatedAtAction(nameof(Get), new { id }, response);
    }

    // PUT api/party/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PartyDto.Request.Update.Single dto)
    {
        dto.Id = id;
        var command = mapper.Map<UpdatePartyCommand>(dto);
        await mediator.Send(command);
        return NoContent();
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
