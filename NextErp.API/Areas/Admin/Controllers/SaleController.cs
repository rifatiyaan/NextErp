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

        // GET api/sale
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchText = null,
            [FromQuery] string? sortBy = null)
        {
            var query = new GetPagedSalesQuery(pageIndex, pageSize, searchText, sortBy);
            var pagedResult = await _mediator.Send(query);

            var dtoList = _mapper.Map<List<Sale.Response.Get.Single>>(pagedResult.Records);

            return Ok(new
            {
                total = pagedResult.Total,
                totalDisplay = pagedResult.TotalDisplay,
                data = dtoList
            });
        }

        // POST api/sale
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Sale.Request.Create.Single dto)
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
