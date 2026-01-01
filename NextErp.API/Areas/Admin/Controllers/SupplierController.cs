using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using NextErp.Application.Queries;

namespace NextErp.API.Web.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SupplierController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        // GET api/supplier/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetSupplierByIdQuery(id);
            var supplier = await _mediator.Send(query);

            if (supplier == null) return NotFound();

            var dto = _mapper.Map<Supplier.Response.Get.Single>(supplier);
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
            var pagedResult = await _mediator.Send(query);

            var dtoList = _mapper.Map<List<Supplier.Response.Get.Single>>(pagedResult.Records);

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
            var command = _mapper.Map<CreateSupplierCommand>(dto);
            var id = await _mediator.Send(command);

            var supplier = await _mediator.Send(new GetSupplierByIdQuery(id));
            var response = _mapper.Map<Supplier.Response.Get.Single>(supplier);

            return CreatedAtAction(nameof(Get), new { id }, response);
        }

        // PUT api/supplier/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Supplier.Request.Update.Single dto)
        {
            var command = _mapper.Map<UpdateSupplierCommand>(dto);
            await _mediator.Send(command);
            return NoContent();
        }

        // DELETE api/supplier/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var command = new SoftDeleteSupplierCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
