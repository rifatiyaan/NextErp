using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Settings;
using NextErp.Application.Queries;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/feature-settings")]
[ApiController]
public class FeatureSettingsController(IMediator mediator) : ControllerBase
{
    // GET api/feature-settings/schema
    [HttpGet("schema")]
    public async Task<IActionResult> GetSchema()
        => Ok(await mediator.Send(new GetFeatureSettingsSchemaQuery()));

    // GET api/feature-settings
    [HttpGet]
    public async Task<IActionResult> GetValues()
        => Ok(await mediator.Send(new GetFeatureSettingsValuesQuery()));

    // PATCH api/feature-settings/{module}
    [HttpPatch("{module}")]
    public async Task<IActionResult> Patch(string module, [FromBody] Dictionary<string, object?> values)
    {
        await mediator.Send(new PatchFeatureSettingsCommand(module, values));
        return NoContent();
    }
}
