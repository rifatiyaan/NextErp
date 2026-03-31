using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using NextErp.Application.Queries;

namespace NextErp.API.Web.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CustomerController(IMediator mediator, IMapper mapper) : ControllerBase
{
    // GET api/customer/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var query = new GetCustomerByIdQuery(id);
        var customer = await mediator.Send(query);

        if (customer == null) return NotFound();

        var dto = mapper.Map<Customer.Response.Get.Single>(customer);
        return Ok(dto);
    }

    // GET api/customer
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] string? sortBy = null)
    {
        var query = new GetPagedCustomersQuery(pageIndex, pageSize, searchText, sortBy);
        var pagedResult = await mediator.Send(query);

        var dtoList = mapper.Map<List<Customer.Response.Get.Single>>(pagedResult.Records);

        return Ok(new
        {
            total = pagedResult.Total,
            totalDisplay = pagedResult.TotalDisplay,
            data = dtoList
        });
    }

    // POST api/customer
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Customer.Request.Create.Single dto)
    {
        var command = mapper.Map<CreateCustomerCommand>(dto);
        var id = await mediator.Send(command);

        var customer = await mediator.Send(new GetCustomerByIdQuery(id));
        var response = mapper.Map<Customer.Response.Get.Single>(customer);

        return CreatedAtAction(nameof(Get), new { id }, response);
    }

    // PUT api/customer/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Customer.Request.Update.Single dto)
    {
        var command = mapper.Map<UpdateCustomerCommand>(dto);
        await mediator.Send(command);
        return NoContent();
    }
}
