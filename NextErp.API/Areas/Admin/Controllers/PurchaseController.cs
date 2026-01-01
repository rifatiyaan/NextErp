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
    public class PurchaseController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public PurchaseController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        // GET api/purchase/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = new GetPurchaseByIdQuery(id);
            var purchase = await _mediator.Send(query);

            if (purchase == null) return NotFound();

            var dto = _mapper.Map<Purchase.Response.Get.Single>(purchase);
            return Ok(dto);
        }

        // GET api/purchase
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchText = null,
            [FromQuery] string? sortBy = null)
        {
            var query = new GetPagedPurchasesQuery(pageIndex, pageSize, searchText, sortBy);
            var pagedResult = await _mediator.Send(query);

            var dtoList = _mapper.Map<List<Purchase.Response.Get.Single>>(pagedResult.Records);

            return Ok(new
            {
                total = pagedResult.Total,
                totalDisplay = pagedResult.TotalDisplay,
                data = dtoList
            });
        }

        // POST api/purchase
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Purchase.Request.Create.Single dto)
        {
            var command = new CreatePurchaseCommand(
                dto.Title,
                dto.PurchaseNumber,
                dto.SupplierId,
                dto.PurchaseDate,
                dto.Items,
                dto.Metadata
            );

            var id = await _mediator.Send(command);

            var purchase = await _mediator.Send(new GetPurchaseByIdQuery(id));
            var response = _mapper.Map<Purchase.Response.Create.Single>(purchase);

            return CreatedAtAction(nameof(Get), new { id }, response);
        }

        // GET api/purchase/report
        [HttpGet("report")]
        public async Task<IActionResult> GetReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? supplierId = null)
        {
            var query = new GetPurchaseReportQuery(startDate, endDate, supplierId);
            var report = await _mediator.Send(query);

            return Ok(report);
        }
    }
}
