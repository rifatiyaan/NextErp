using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Queries.Ecommerce;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class EcommerceController(IMediator mediator) : ControllerBase
{
    [HttpGet("publication")]
    public async Task<IActionResult> GetPublication() =>
        Ok(await mediator.Send(new GetEcommercePublicationQuery()));

    [HttpPut("publication")]
    public async Task<IActionResult> SetPublication([FromBody] SetEcommercePublicationCommand command)
    {
        await mediator.Send(command);
        return NoContent();
    }
}
