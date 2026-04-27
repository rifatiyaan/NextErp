using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using NextErp.Application.Services;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Payment
{
    public class RecordSalePaymentHandler(
        IApplicationDbContext dbContext)
        : IRequestHandler<RecordSalePaymentCommand, Guid>
    {
        public async Task<Guid> Handle(RecordSalePaymentCommand request, CancellationToken cancellationToken = default)
        {
            SalePaymentRules.RequirePositiveAmount(request.Amount);

            var sale = await dbContext.Sales
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken);

            if (sale == null)
                throw new InvalidOperationException($"Sale {request.SaleId} was not found.");

            var alreadyPaid = sale.Payments.Sum(p => p.Amount);
            SalePaymentRules.RequireNotOverSaleTotal(sale.FinalAmount, alreadyPaid, request.Amount);

            var paidAt = request.PaidAt ?? DateTime.UtcNow;
            var payment = new Entities.SalePayment
            {
                Id = Guid.NewGuid(),
                Title = $"Payment — {paidAt:yyyy-MM-dd}",
                SaleId = sale.Id,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                PaidAt = paidAt,
                Reference = request.Reference,
                TenantId = sale.TenantId,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.SalePayments.AddAsync(payment, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return payment.Id;
        }
    }
}
