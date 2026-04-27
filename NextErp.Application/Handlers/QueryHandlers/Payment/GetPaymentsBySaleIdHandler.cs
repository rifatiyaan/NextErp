using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using PaymentContracts = NextErp.Application.DTOs.Payment;

namespace NextErp.Application.Handlers.QueryHandlers.Payment
{
    public class GetPaymentsBySaleIdHandler(IApplicationDbContext dbContext, IMapper mapper)
        : IRequestHandler<GetPaymentsBySaleIdQuery, IReadOnlyList<PaymentContracts.Response.Line>>
    {
        public async Task<IReadOnlyList<PaymentContracts.Response.Line>> Handle(
            GetPaymentsBySaleIdQuery request,
            CancellationToken cancellationToken = default)
        {
            var rows = await dbContext.SalePayments
                .AsNoTracking()
                .Where(p => p.SaleId == request.SaleId)
                .OrderBy(p => p.PaidAt)
                .ThenBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            return mapper.Map<List<PaymentContracts.Response.Line>>(rows).AsReadOnly();
        }
    }
}
