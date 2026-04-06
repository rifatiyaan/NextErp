using MediatR;
using NextErp.Application.Common.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Commands
{
    public record RecordSalePaymentCommand(
        Guid SaleId,
        decimal Amount,
        PaymentMethodType PaymentMethod,
        DateTime? PaidAt,
        string? Reference) : IRequest<Guid>, ITransactionalRequest;
}
