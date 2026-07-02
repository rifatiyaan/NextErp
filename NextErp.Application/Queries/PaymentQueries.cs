using MediatR;
using PaymentDto = NextErp.Application.DTOs.Payment;

namespace NextErp.Application.Queries
{
    public record GetPaymentsBySaleIdQuery(Guid SaleId) : IRequest<IReadOnlyList<PaymentDto.PaymentLineResponse>>;
}
