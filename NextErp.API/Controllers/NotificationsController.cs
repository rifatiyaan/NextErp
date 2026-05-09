using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Notifications;
using NextErp.Application.Queries.Notifications;
using NotificationDto = NextErp.Application.DTOs.Notification;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class NotificationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NotificationDto.Response.List>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] string? type = null,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetNotificationsQuery(page, pageSize, unreadOnly, type), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct = default)
    {
        await sender.Send(new MarkNotificationReadCommand(id), ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct = default)
    {
        await sender.Send(new MarkAllNotificationsReadCommand(), ct);
        return NoContent();
    }
}
