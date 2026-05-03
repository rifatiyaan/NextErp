using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Payment;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Payment;

public class RecordSalePaymentHandlerTests : HandlerTestBase
{
    private RecordSalePaymentHandler BuildHandler() => new(Db);

    private async Task<Guid> SeedSaleAsync(decimal finalAmount = 500m)
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        var saleId = Guid.NewGuid();
        Db.Sales.Add(new SaleBuilder()
            .WithId(saleId)
            .WithFinalAmount(finalAmount)
            .WithTotalAmount(finalAmount)
            .WithTenant(TenantId)
            .WithBranch(BranchId)
            .Build());
        await Db.SaveChangesAsync();
        return saleId;
    }

    [Fact]
    public async Task Happy_path_records_payment_and_returns_id()
    {
        var saleId = await SeedSaleAsync(finalAmount: 500m);
        var sut = BuildHandler();

        var paidAt = DateTime.UtcNow.AddMinutes(-1);
        var paymentId = await sut.Handle(
            new RecordSalePaymentCommand(saleId, 200m, PaymentMethodType.Cash, paidAt, "Ref-1"),
            CancellationToken.None);

        paymentId.Should().NotBe(Guid.Empty);

        var payment = await Db.SalePayments.AsNoTracking().FirstAsync(p => p.Id == paymentId);
        payment.SaleId.Should().Be(saleId);
        payment.Amount.Should().Be(200m);
        payment.PaymentMethod.Should().Be(PaymentMethodType.Cash);
        payment.PaidAt.Should().BeCloseTo(paidAt, TimeSpan.FromSeconds(1));
        payment.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task Sale_not_found_throws_invalid_operation()
    {
        await SeedSaleAsync();
        var sut = BuildHandler();

        var act = async () => await sut.Handle(
            new RecordSalePaymentCommand(Guid.NewGuid(), 50m, PaymentMethodType.Cash, null, null),
            CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Multiple_payments_accumulate()
    {
        var saleId = await SeedSaleAsync(finalAmount: 500m);
        var sut = BuildHandler();

        await sut.Handle(
            new RecordSalePaymentCommand(saleId, 150m, PaymentMethodType.Cash, null, null),
            CancellationToken.None);
        await sut.Handle(
            new RecordSalePaymentCommand(saleId, 100m, PaymentMethodType.Card, null, null),
            CancellationToken.None);

        var payments = await Db.SalePayments.AsNoTracking()
            .Where(p => p.SaleId == saleId).ToListAsync();
        payments.Should().HaveCount(2);
        payments.Sum(p => p.Amount).Should().Be(250m);
    }

    [Fact]
    public async Task PaymentMethod_and_reference_are_preserved()
    {
        var saleId = await SeedSaleAsync(finalAmount: 500m);
        var sut = BuildHandler();

        var paymentId = await sut.Handle(
            new RecordSalePaymentCommand(saleId, 75m, PaymentMethodType.BankTransfer, null, "INV-9001"),
            CancellationToken.None);

        var payment = await Db.SalePayments.AsNoTracking().FirstAsync(p => p.Id == paymentId);
        payment.PaymentMethod.Should().Be(PaymentMethodType.BankTransfer);
        payment.Reference.Should().Be("INV-9001");
    }
}

