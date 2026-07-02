using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using NextErp.Application.Queries;
using PaymentContracts = NextErp.Application.DTOs.Payment;

namespace NextErp.Application.Handlers.QueryHandlers.Payment
{
    public class GetPaymentsBySaleIdHandler(IApplicationDbContext dbContext)
        : IRequestHandler<GetPaymentsBySaleIdQuery, IReadOnlyList<PaymentContracts.PaymentLineResponse>>
    {
        public async Task<IReadOnlyList<PaymentContracts.PaymentLineResponse>> Handle(
            GetPaymentsBySaleIdQuery request,
            CancellationToken cancellationToken = default)
        {
            var rows = await dbContext.SalePayments
                .AsNoTracking()
                .Where(p => p.SaleId == request.SaleId)
                .OrderBy(p => p.PaidAt)
                .ThenBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            return rows.Select(r => r.ToResponse()).ToList().AsReadOnly();
        }
    }
}
