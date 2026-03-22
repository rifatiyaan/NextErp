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
    public class SaleController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SaleController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        // GET api/sale/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = new GetSaleByIdQuery(id);
            var sale = await _mediator.Send(query);

            if (sale == null) return NotFound();

            var dto = _mapper.Map<Sale.Response.Get.Single>(sale);
            return Ok(dto);
        }

        // GET api/sale  (supports page + pageSize or pageIndex + pageSize)
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int? page = null,
            [FromQuery] int? pageIndex = null,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchText = null,
            [FromQuery] string? sortBy = null)
        {
            var effectivePage = page ?? pageIndex ?? 1;
            if (effectivePage < 1) effectivePage = 1;
            if (pageSize < 1) pageSize = 10;

            var query = new GetPagedSalesQuery(effectivePage, pageSize, searchText, sortBy);
            var pagedResult = await _mediator.Send(query);

            return Ok(new
            {
                total = pagedResult.Total,
                totalDisplay = pagedResult.TotalDisplay,
                page = effectivePage,
                pageSize,
                data = pagedResult.Records
            });
        }

        // POST api/sale
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Sale.Request.Create.Single dto)
        {
            try
            {
                var command = new CreateSaleCommand(
                    dto.CustomerId,
                    dto.TotalAmount,
                    dto.Discount,
                    dto.Tax,
                    dto.FinalAmount,
                    dto.PaymentMethod,
                    dto.Items
                );

                var id = await _mediator.Send(command);

                var sale = await _mediator.Send(new GetSaleByIdQuery(id));
                var response = _mapper.Map<Sale.Response.Get.Single>(sale);

                return CreatedAtAction(nameof(Get), new { id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/sale/report
        [HttpGet("report")]
        public async Task<IActionResult> GetReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? customerId = null)
        {
            var query = new GetSalesReportQuery(startDate, endDate, customerId);
            var report = await _mediator.Send(query);

            return Ok(report);
        }
    }
}
