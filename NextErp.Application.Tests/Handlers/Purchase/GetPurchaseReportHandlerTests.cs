using NextErp.Application.Handlers.QueryHandlers.Purchase;
using NextErp.Application.Queries;
using PurchaseEntity = NextErp.Domain.Entities.Purchase;

namespace NextErp.Application.Tests.Handlers.Purchase;

public class GetPurchaseReportHandlerTests : HandlerTestBase
{
    private GetPurchaseReportHandler BuildHandler() => new(Db);

    private async Task<(Guid PartyA, Guid PartyB)> SeedPurchasesAsync()
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());

        var partyA = Guid.NewGuid();
        var partyB = Guid.NewGuid();
        Db.Parties.Add(new PartyBuilder()
            .WithId(partyA).WithTitle("Supplier A").AsSupplier().WithTenant(TenantId).WithBranch(BranchId).Build());
        Db.Parties.Add(new PartyBuilder()
            .WithId(partyB).WithTitle("Supplier B").AsSupplier().WithTenant(TenantId).WithBranch(BranchId).Build());

        // 5 purchases — 3 inside [2026-01-01, 2026-01-31], 2 outside.
        AddPurchase(new DateTime(2026, 1, 5),  partyA, total: 100m, num: "P-1");
        AddPurchase(new DateTime(2026, 1, 15), partyB, total: 200m, num: "P-2");
        AddPurchase(new DateTime(2026, 1, 25), partyA, total: 300m, num: "P-3");
        AddPurchase(new DateTime(2025, 12, 20), partyA, total: 999m, num: "P-OUT-1"); // before
        AddPurchase(new DateTime(2026, 2, 10),  partyB, total: 999m, num: "P-OUT-2"); // after

        await Db.SaveChangesAsync();
        return (partyA, partyB);
    }

    private void AddPurchase(DateTime purchaseDate, Guid partyId, decimal total, string num)
    {
        Db.Purchases.Add(new PurchaseEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Purchase",
            PurchaseNumber = num,
            PartyId = partyId,
            PurchaseDate = purchaseDate,
            TotalAmount = total,
            Discount = 0m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            TenantId = TenantId,
            BranchId = BranchId,
        });
    }

    [Fact]
    public async Task Date_range_filter_returns_only_purchases_in_range()
    {
        await SeedPurchasesAsync();
        var sut = BuildHandler();

        var report = await sut.Handle(
            new GetPurchaseReportQuery(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), PartyId: null),
            CancellationToken.None);

        report.TotalPurchases.Should().Be(3);
        report.Purchases.Should().HaveCount(3);
    }

    [Fact]
    public async Task PartyId_filter_returns_only_matching_supplier()
    {
        var (partyA, _) = await SeedPurchasesAsync();
        var sut = BuildHandler();

        var report = await sut.Handle(
            new GetPurchaseReportQuery(
                new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), PartyId: partyA),
            CancellationToken.None);

        report.TotalPurchases.Should().Be(2);
        report.Purchases.Should().AllSatisfy(p => p.PartyId.Should().Be(partyA));
    }

    [Fact]
    public async Task TotalPurchaseAmount_aggregates_TotalAmount_in_range()
    {
        await SeedPurchasesAsync();
        var sut = BuildHandler();

        var report = await sut.Handle(
            new GetPurchaseReportQuery(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), PartyId: null),
            CancellationToken.None);

        report.TotalPurchaseAmount.Should().Be(600m); // 100 + 200 + 300
    }
}

