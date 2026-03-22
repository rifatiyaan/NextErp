using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.DTOs;
using NextErp.Application.Queries;

namespace NextErp.API.Web.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public StockController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>Stock ledger row for a single SKU (shared id with ProductVariant).</summary>
        [HttpGet("variant/{productVariantId:int}")]
        public async Task<IActionResult> GetByProductVariantId(int productVariantId)
        {
            var stock = await _mediator.Send(new GetStockByProductVariantIdQuery(productVariantId));

            if (stock == null)
                return NotFound();

            return Ok(_mapper.Map<Stock.Response.Single>(stock));
        }

        /// <summary>All variant stock rows for a product.</summary>
        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            var stocks = await _mediator.Send(new GetStocksByProductIdQuery(productId));
            return Ok(_mapper.Map<List<Stock.Response.Single>>(stocks));
        }

        [HttpGet("report/current")]
        public async Task<IActionResult> GetCurrentStockReport()
        {
            var report = await _mediator.Send(new GetCurrentStockReportQuery());
            return Ok(report);
        }

        [HttpGet("report/low")]
        public async Task<IActionResult> GetLowStockReport()
        {
            var report = await _mediator.Send(new GetLowStockReportQuery());
            return Ok(report);
        }
    }
}
