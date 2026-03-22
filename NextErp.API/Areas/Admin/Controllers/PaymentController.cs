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
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public PaymentController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>List payments recorded for a sale (ordered by paid date).</summary>
        [HttpGet("sale/{saleId:guid}")]
        public async Task<IActionResult> GetBySaleId(Guid saleId)
        {
            var lines = await _mediator.Send(new GetPaymentsBySaleIdQuery(saleId));
            return Ok(lines);
        }

        /// <summary>Record a partial or full payment. Sum of payments cannot exceed the sale final amount.</summary>
        [HttpPost]
        public async Task<IActionResult> Record([FromBody] Payment.Request.Record dto)
        {
            try
            {
                var command = new RecordSalePaymentCommand(
                    dto.SaleId,
                    dto.Amount,
                    dto.PaymentMethod,
                    dto.PaidAt,
                    dto.Reference);

                var id = await _mediator.Send(command);
                var lines = await _mediator.Send(new GetPaymentsBySaleIdQuery(dto.SaleId));
                var created = lines.FirstOrDefault(p => p.Id == id);

                return created != null
                    ? CreatedAtAction(nameof(GetBySaleId), new { saleId = dto.SaleId }, created)
                    : Ok(new { id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
