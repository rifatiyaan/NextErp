using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Queries.Dashboard;

namespace NextErp.API.Controllers;

/// <summary>
/// Dashboard aggregate endpoint. Returns the entire homepage payload in a
/// single response so the dashboard page can subscribe with one React-Query
/// key. All sub-section limits are tunable via query params for future
/// "compact" or "wide" variants of the page.
/// </summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DashboardController(IMediator mediator) : ControllerBase
{
    // GET api/dashboard/overview?revenueTrendMonths=12&topProductsLimit=5&...
    [HttpGet("overview")]
    public async Task<IActionResult> Overview(
        [FromQuery] int revenueTrendMonths = 12,
        [FromQuery] int topProductsLimit = 5,
        [FromQuery] int topCustomersLimit = 5,
        [FromQuery] int recentTransactionsLimit = 10,
        [FromQuery] int activityLimit = 10,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetDashboardOverviewQuery(
                revenueTrendMonths,
                topProductsLimit,
                topCustomersLimit,
                recentTransactionsLimit,
                activityLimit),
            ct);
        return Ok(result);
    }
}
