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
    public class VariationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public VariationController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet("product/{productId}/options")]
        public async Task<IActionResult> GetOptionsByProduct(int productId)
        {
            var query = new GetVariationOptionsByProductIdQuery(productId);
            var options = await _mediator.Send(query);
            var dtoList = _mapper.Map<List<ProductVariation.Response.VariationOptionDto>>(options);
            return Ok(dtoList);
        }

        [HttpPost("product/{productId}/options")]
        public async Task<IActionResult> CreateOption(int productId, [FromBody] ProductVariation.Request.VariationOptionDto dto)
        {
            var command = new CreateVariationOptionCommand(productId, dto.Name, dto.DisplayOrder);
            var optionId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetOption), new { id = optionId }, new { id = optionId });
        }

        [HttpGet("options/{id}")]
        public async Task<IActionResult> GetOption(int id)
        {
            var query = new GetVariationOptionByIdQuery(id);
            var option = await _mediator.Send(query);
            if (option == null) return NotFound();
            var dto = _mapper.Map<ProductVariation.Response.VariationOptionDto>(option);
            return Ok(dto);
        }

        [HttpPut("options/{id}")]
        public async Task<IActionResult> UpdateOption(int id, [FromBody] ProductVariation.Request.VariationOptionDto dto)
        {
            var command = new UpdateVariationOptionCommand(id, dto.Name, dto.DisplayOrder);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("options/{id}")]
        public async Task<IActionResult> DeleteOption(int id)
        {
            var command = new DeleteVariationOptionCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("options/{optionId}/values")]
        public async Task<IActionResult> CreateValue(int optionId, [FromBody] ProductVariation.Request.VariationValueDto dto)
        {
            var command = new CreateVariationValueCommand(optionId, dto.Value, dto.DisplayOrder);
            var valueId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetValue), new { id = valueId }, new { id = valueId });
        }

        [HttpGet("values/{id}")]
        public async Task<IActionResult> GetValue(int id)
        {
            var query = new GetVariationValueByIdQuery(id);
            var value = await _mediator.Send(query);
            if (value == null) return NotFound();
            var dto = _mapper.Map<ProductVariation.Response.VariationValueDto>(value);
            return Ok(dto);
        }

        [HttpPut("values/{id}")]
        public async Task<IActionResult> UpdateValue(int id, [FromBody] ProductVariation.Request.VariationValueDto dto)
        {
            var command = new UpdateVariationValueCommand(id, dto.Value, dto.DisplayOrder);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("values/{id}")]
        public async Task<IActionResult> DeleteValue(int id)
        {
            var command = new DeleteVariationValueCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}

