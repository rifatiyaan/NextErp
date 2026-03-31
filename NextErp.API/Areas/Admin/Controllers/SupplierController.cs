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
public class SupplierController(IMediator mediator, IMapper mapper) : ControllerBase
{
    // GET api/supplier/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var query = new GetSupplierByIdQuery(id);
        var supplier = await mediator.Send(query);

        if (supplier == null) return NotFound();

        var dto = mapper.Map<Supplier.Response.Get.Single>(supplier);
        return Ok(dto);
    }

    // GET api/supplier
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] string? sortBy = null)
    {
        var query = new GetPagedSuppliersQuery(pageIndex, pageSize, searchText, sortBy);
        var pagedResult = await mediator.Send(query);

        var dtoList = mapper.Map<List<Supplier.Response.Get.Single>>(pagedResult.Records);

        return Ok(new
        {
            total = pagedResult.Total,
            totalDisplay = pagedResult.TotalDisplay,
            data = dtoList
        });
    }

    // POST api/supplier
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Supplier.Request.Create.Single dto)
    {
        var command = mapper.Map<CreateSupplierCommand>(dto);
        var id = await mediator.Send(command);

        var supplier = await mediator.Send(new GetSupplierByIdQuery(id));
        var response = mapper.Map<Supplier.Response.Get.Single>(supplier);

        return CreatedAtAction(nameof(Get), new { id }, response);
    }

    // PUT api/supplier/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Supplier.Request.Update.Single dto)
    {
        var command = mapper.Map<UpdateSupplierCommand>(dto);
        await mediator.Send(command);
        return NoContent();
    }
}
