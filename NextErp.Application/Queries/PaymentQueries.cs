using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Queries
{
    public record GetPaymentsBySaleIdQuery(Guid SaleId) : IRequest<IReadOnlyList<Payment.Response.Line>>;
}
