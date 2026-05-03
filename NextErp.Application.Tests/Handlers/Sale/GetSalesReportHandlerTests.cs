using NextErp.Application.Handlers.QueryHandlers.Sale;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Sale;

public class GetSalesReportHandlerTests : HandlerTestBase
{
    private GetSalesReportHandler BuildHandler() => new(Db);

    private async Task<(Guid PartyA, Guid PartyB)> SeedSalesAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        var partyA = Guid.NewGuid();
        var partyB = Guid.NewGuid();
        Db.Parties.Add(new PartyBuilder()
            .WithId(partyA).WithTitle("Customer A").WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Parties.Add(new PartyBuilder()
            .WithId(partyB).WithTitle("Customer B").WithTenant(TenantId).WithBranch(BranchId).Build());

        // 5 sales — 3 inside [2026-01-01, 2026-01-31], 2 outside.
        AddSale(new DateTime(2026, 1, 5),  partyA, total: 100m, finalAmount: 100m);
        AddSale(new DateTime(2026, 1, 15), partyB, total: 200m, finalAmount: 200m);
        AddSale(new DateTime(2026, 1, 25), partyA, total: 300m, finalAmount: 300m);
        AddSale(new DateTime(2025, 12, 20), partyA, total: 999m, finalAmount: 999m); // before
        AddSale(new DateTime(2026, 2, 10),  partyB, total: 999m, finalAmount: 999m); // after

        await Db.SaveChangesAsync();
        return (partyA, partyB);
    }

    private void AddSale(DateTime saleDate, Guid partyId, decimal total, decimal finalAmount)
    {
        Db.Sales.Add(new SaleBuilder()
            .WithSaleDate(saleDate)
            .WithParty(partyId)
            .WithTotalAmount(total)
            .WithFinalAmount(finalAmount)
            .WithTenant(TenantId)
            .WithBranch(BranchId)
            .Build());
    }

    [Fact]
    public async Task Date_range_filter_returns_only_sales_in_range()
    {
        await SeedSalesAsync();
        var sut = BuildHandler();

        var report = await sut.Handle(
            new GetSalesReportQuery(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), PartyId: null),
            CancellationToken.None);

        report.TotalSales.Should().Be(3);
        report.Sales.Should().HaveCount(3);
    }

    [Fact]
    public async Task TotalSalesAmount_aggregates_TotalAmount_in_range()
    {
        await SeedSalesAsync();
        var sut = BuildHandler();

        var report = await sut.Handle(
            new GetSalesReportQuery(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), PartyId: null),
            CancellationToken.None);

        report.TotalSalesAmount.Should().Be(600m); // 100 + 200 + 300
    }

    [Fact]
    public async Task PartyId_filter_returns_only_matching_party()
    {
        var (partyA, _) = await SeedSalesAsync();
        var sut = BuildHandler();

        var report = await sut.Handle(
            new GetSalesReportQuery(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), PartyId: partyA),
            CancellationToken.None);

        report.TotalSales.Should().Be(2); // partyA had 2 in-range (Jan 5, Jan 25)
        report.Sales.Should().AllSatisfy(s => s.PartyId.Should().Be(partyA));

        // And when PartyId is null, all in-range sales are returned.
        var allReport = await sut.Handle(
            new GetSalesReportQuery(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), PartyId: null),
            CancellationToken.None);
        allReport.TotalSales.Should().Be(3);
    }
}

