using AutoMapper;
using NextErp.Application.Handlers.QueryHandlers.Payment;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Payment;

public class GetPaymentsBySaleIdHandlerTests : HandlerTestBase
{
    private static readonly IMapper Mapper = BuildMapper();

    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
            c.AddMaps(typeof(NextErp.Application.ApplicationAssemblyMarker).Assembly));
        return cfg.CreateMapper();
    }

    private GetPaymentsBySaleIdHandler BuildHandler() => new(Db, Mapper);

    [Fact]
    public async Task Payments_for_sale_returned_and_empty_for_unknown_sale()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());

        var saleA = Guid.NewGuid();
        var saleB = Guid.NewGuid();
        Db.Sales.Add(new SaleBuilder()
            .WithId(saleA).WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Sales.Add(new SaleBuilder()
            .WithId(saleB).WithTenant(TenantId).WithBranch(BranchId).Build());

        Db.SalePayments.Add(new SalePayment
        {
            Id = Guid.NewGuid(), SaleId = saleA, Amount = 100m,
            PaidAt = DateTime.UtcNow, Title = "Cash",
            CreatedAt = DateTime.UtcNow, TenantId = TenantId,
        });
        Db.SalePayments.Add(new SalePayment
        {
            Id = Guid.NewGuid(), SaleId = saleA, Amount = 50m,
            PaidAt = DateTime.UtcNow.AddSeconds(1), Title = "Card",
            CreatedAt = DateTime.UtcNow, TenantId = TenantId,
        });
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var forA = await sut.Handle(new GetPaymentsBySaleIdQuery(saleA), CancellationToken.None);
        forA.Should().HaveCount(2);

        var forB = await sut.Handle(new GetPaymentsBySaleIdQuery(saleB), CancellationToken.None);
        forB.Should().BeEmpty();

        var unknown = await sut.Handle(new GetPaymentsBySaleIdQuery(Guid.NewGuid()), CancellationToken.None);
        unknown.Should().BeEmpty();
    }
}

