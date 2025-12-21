using AutoMapper;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using NextErp.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NextErp.API.Web.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly NextErp.Application.Interfaces.IImageService _imageService;

        public ProductController(IMediator mediator, IMapper mapper, NextErp.Application.Interfaces.IImageService imageService)
        {
            _mediator = mediator;
            _mapper = mapper;
            _imageService = imageService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetProductByIdQuery(id);
            var product = await _mediator.Send(query);

            if (product == null)
                return NotFound();

            var dto = _mapper.Map<Product.Response.Get.Single>(product);
            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchText = null,
            [FromQuery] string? sortBy = null)
        {
            var query = new GetPagedProductsQuery(pageIndex, pageSize, searchText, sortBy);
            var pagedResult = await _mediator.Send(query);

            var dtoList = _mapper.Map<List<Product.Response.Get.Single>>(pagedResult.Records);

            return Ok(new
            {
                total = pagedResult.Total,
                totalDisplay = pagedResult.TotalDisplay,
                data = dtoList
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Product.Request.Create.Single dto)
        {
            if (dto.Image != null)
            {
                dto.ImageUrl = await _imageService.UploadImageAsync(dto.Image);
            }

            var command = _mapper.Map<CreateProductCommand>(dto);
            var id = await _mediator.Send(command);

            var product = await _mediator.Send(new GetProductByIdQuery(id));
            var response = _mapper.Map<Product.Response.Get.Single>(product);

            return CreatedAtAction(nameof(Get), new { id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] Product.Request.Update.Single dto)
        {
            if (dto.Image != null)
            {
                dto.ImageUrl = await _imageService.UploadImageAsync(dto.Image);
            }

            var command = _mapper.Map<UpdateProductCommand>(dto, opts => opts.Items["Id"] = id);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var command = new SoftDeleteProductCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
