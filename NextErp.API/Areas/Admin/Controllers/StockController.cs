using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Queries;

namespace NextErp.API.Web.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StockController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/stock/product/{productId}
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            var query = new GetStockByProductIdQuery(productId);
            var stock = await _mediator.Send(query);

            if (stock == null) return NotFound();

            return Ok(stock);
        }

        // GET api/stock/report/current
        [HttpGet("report/current")]
        public async Task<IActionResult> GetCurrentStockReport()
        {
            var query = new GetCurrentStockReportQuery();
            var report = await _mediator.Send(query);

            return Ok(report);
        }

        // GET api/stock/report/low
        [HttpGet("report/low")]
        public async Task<IActionResult> GetLowStockReport()
        {
            var query = new GetLowStockReportQuery();
            var report = await _mediator.Send(query);

            return Ok(report);
        }
    }
}
