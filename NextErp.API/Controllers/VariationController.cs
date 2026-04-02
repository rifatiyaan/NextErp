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
public class VariationController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet("options")]
    public async Task<IActionResult> GetAllOptions()
    {
        var query = new GetAllVariationOptionsQuery();
        var options = await mediator.Send(query);
        var dtoList = mapper.Map<List<ProductVariation.Response.VariationOptionDto>>(options);
        return Ok(dtoList);
    }

    [HttpPost("options")]
    public async Task<IActionResult> CreateOption([FromBody] ProductVariation.Request.VariationOptionDto dto)
    {
        var tenantId = Guid.TryParse(User.FindFirst("TenantId")?.Value, out var tid) ? tid : Guid.Empty;
        var command = new CreateVariationOptionCommandGlobal(dto.Name, dto.DisplayOrder, tenantId);
        var optionId = await mediator.Send(command);
        return CreatedAtAction(nameof(GetOption), new { id = optionId }, new { id = optionId });
    }

    [HttpGet("options/{id}")]
    public async Task<IActionResult> GetOption(int id)
    {
        var query = new GetVariationOptionByIdQuery(id);
        var option = await mediator.Send(query);
        if (option == null) return NotFound();
        var dto = mapper.Map<ProductVariation.Response.VariationOptionDto>(option);
        return Ok(dto);
    }

    [HttpPut("options/{id}")]
    public async Task<IActionResult> UpdateOption(int id, [FromBody] ProductVariation.Request.VariationOptionDto dto)
    {
        var command = new UpdateVariationOptionCommand(id, dto.Name, dto.DisplayOrder);
        await mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("options/{id}")]
    public async Task<IActionResult> DeleteOption(int id)
    {
        var command = new DeleteVariationOptionCommand(id);
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPost("options/{optionId}/values")]
    public async Task<IActionResult> CreateValue(int optionId, [FromBody] ProductVariation.Request.VariationValueDto dto)
    {
        var command = new CreateVariationValueCommand(optionId, dto.Value, dto.DisplayOrder);
        var valueId = await mediator.Send(command);
        return CreatedAtAction(nameof(GetValue), new { id = valueId }, new { id = valueId });
    }

    [HttpGet("values/{id}")]
    public async Task<IActionResult> GetValue(int id)
    {
        var query = new GetVariationValueByIdQuery(id);
        var value = await mediator.Send(query);
        if (value == null) return NotFound();
        var dto = mapper.Map<ProductVariation.Response.VariationValueDto>(value);
        return Ok(dto);
    }

    [HttpPut("values/{id}")]
    public async Task<IActionResult> UpdateValue(int id, [FromBody] ProductVariation.Request.VariationValueDto dto)
    {
        var command = new UpdateVariationValueCommand(id, dto.Value, dto.DisplayOrder);
        await mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("values/{id}")]
    public async Task<IActionResult> DeleteValue(int id)
    {
        var command = new DeleteVariationValueCommand(id);
        await mediator.Send(command);
        return NoContent();
    }

    [HttpGet("product/{productId}/options")]
    public async Task<IActionResult> GetOptionsByProduct(int productId)
    {
        var query = new GetVariationOptionsByProductIdQuery(productId);
        var options = await mediator.Send(query);
        var dtoList = mapper.Map<List<ProductVariation.Response.VariationOptionDto>>(options);
        return Ok(dtoList);
    }

    [HttpPost("product/{productId}/assign-option")]
    public async Task<IActionResult> AssignOptionToProduct(int productId, [FromBody] AssignVariationOptionRequest body)
    {
        var command = new AssignVariationOptionToProductCommand(productId, body.VariationOptionId, body.DisplayOrder);
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetOptionsByProduct), new { productId }, new { id });
    }

    [HttpDelete("product/{productId}/assign-option/{variationOptionId}")]
    public async Task<IActionResult> UnassignOptionFromProduct(int productId, int variationOptionId)
    {
        var command = new UnassignVariationOptionFromProductCommand(productId, variationOptionId);
        await mediator.Send(command);
        return NoContent();
    }

    [HttpGet("bulk/options")]
    public async Task<IActionResult> GetAllDistinctOptions()
    {
        var query = new GetAllDistinctVariationOptionsQuery();
        var options = await mediator.Send(query);
        return Ok(options);
    }

    public sealed class AssignVariationOptionRequest
    {
        public int VariationOptionId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
