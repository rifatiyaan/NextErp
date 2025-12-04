using AutoMapper;
using EcommerceApplicationWeb.Application.Commands;
using EcommerceApplicationWeb.Application.DTOs;
using EcommerceApplicationWeb.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApplicationWeb.Web.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CategoryController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        // GET api/category/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetCategoryByIdQuery(id);
            var category = await _mediator.Send(query);

            if (category == null) return NotFound();

            var dto = _mapper.Map<CategoryResponseDto>(category);
            return Ok(dto);
        }

        // GET api/category
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchText = null,
            [FromQuery] string? sortBy = null)
        {
            var query = new GetPagedCategoriesQuery(pageIndex, pageSize, searchText, sortBy);
            var pagedResult = await _mediator.Send(query);

            var dtoList = _mapper.Map<List<CategoryResponseDto>>(pagedResult.Records);

            return Ok(new
            {
                total = pagedResult.Total,
                totalDisplay = pagedResult.TotalDisplay,
                data = dtoList
            });
        }

        // POST api/category
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryRequestDto dto)
        {
            var command = _mapper.Map<CreateCategoryCommand>(dto);
            var id = await _mediator.Send(command);

            var category = await _mediator.Send(new GetCategoryByIdQuery(id));
            var response = _mapper.Map<CategoryResponseDto>(category);

            return CreatedAtAction(nameof(Get), new { id }, response);
        }

        // PUT api/category/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryRequestDto dto)
        {
            var command = _mapper.Map<UpdateCategoryCommand>(dto, opts => opts.Items["Id"] = id);
            await _mediator.Send(command);
            return NoContent();
        }

        // DELETE api/category/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var command = new SoftDeleteCategoryCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
