using System.Security.Claims;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Module;
using NextErp.Application.DTOs;
using NextErp.Application.Queries.Module;

namespace NextErp.API.Web.Api;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ModuleController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpGet("user-menu")]
    public async Task<IActionResult> GetUserMenu()
    {
        // Get user ID from claims
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in claims");
        }

        // Extract tenant ID from claims
        var tenantIdClaim = User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            tenantId = Guid.Empty;
        }

        var query = new GetMenuByUserQuery(userId, tenantId);
        var menuItems = await mediator.Send(query);

        return Ok(menuItems);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? tenantId = null)
    {
        _ = tenantId ?? Guid.Empty;

        var query = new GetAllModulesQuery();
        var modules = await mediator.Send(query);

        return Ok(modules);
    }

    [HttpGet("by-type/{type}")]
    public async Task<IActionResult> GetByType(int type, [FromQuery] Guid? tenantId = null)
    {
        _ = tenantId ?? Guid.Empty;

        var query = new GetModulesByTypeQuery(type);
        var modules = await mediator.Send(query);

        return Ok(modules);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var query = new GetModuleByIdQuery(id);
        var module = await mediator.Send(query);

        if (module == null)
            return NotFound();

        return Ok(module);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Module.Request.Create.Single dto)
    {
        var command = new CreateModuleCommand(dto);
        var id = await mediator.Send(command);

        var module = await mediator.Send(new GetModuleByIdQuery(id));
        return CreatedAtAction(nameof(Get), new { id }, module);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] Module.Request.Create.Bulk dto)
    {
        var command = new CreateBulkModulesCommand(dto);
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Module.Request.Update.Single dto)
    {
        var command = new UpdateModuleCommand(id, dto);
        await mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteModuleCommand(id);
        await mediator.Send(command);
        return NoContent();
    }
}
