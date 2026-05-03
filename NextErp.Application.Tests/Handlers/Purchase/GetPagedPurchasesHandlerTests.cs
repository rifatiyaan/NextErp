using NextErp.Application.Handlers.QueryHandlers.Purchase;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Purchase;

public class GetPagedPurchasesHandlerTests : HandlerTestBase
{
    private GetPagedPurchasesHandler BuildHandler() => new(Db, BranchProvider);

    private async Task SeedAsync(int countCurrent = 15, int countOtherBranch = 0, Guid? otherBranchId = null)
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        var partyId = Guid.NewGuid();
        Db.Parties.Add(new PartyBuilder()
            .WithId(partyId).AsSupplier().WithTenant(TenantId).WithBranch(BranchId).Build());

        for (var i = 0; i < countCurrent; i++)
        {
            Db.Purchases.Add(new Domain.Entities.Purchase
            {
                Id = Guid.NewGuid(),
                Title = $"Purchase {i}",
                PurchaseNumber = $"P-{i:D3}",
                PartyId = partyId,
                PurchaseDate = new DateTime(2026, 1, 1).AddDays(i),
                TotalAmount = 100m + i,
                Discount = 0m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                TenantId = TenantId,
                BranchId = BranchId,
            });
        }

        if (countOtherBranch > 0 && otherBranchId.HasValue)
        {
            Db.Branches.Add(new BranchBuilder().WithId(otherBranchId.Value).WithTenant(TenantId).Build());
            for (var i = 0; i < countOtherBranch; i++)
            {
                Db.Purchases.Add(new Domain.Entities.Purchase
                {
                    Id = Guid.NewGuid(),
                    Title = $"Other {i}",
                    PurchaseNumber = $"OB-{i:D3}",
                    PartyId = null,
                    PurchaseDate = DateTime.UtcNow,
                    TotalAmount = 200m,
                    Discount = 0m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    TenantId = TenantId,
                    BranchId = otherBranchId.Value,
                });
            }
        }

        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Pagination_page_2_size_5_returns_5_rows_and_total_15()
    {
        await SeedAsync(countCurrent: 15);
        var sut = BuildHandler();

        var (records, total, totalDisplay) = await sut.Handle(
            new GetPagedPurchasesQuery(PageIndex: 2, PageSize: 5, SearchText: null, SortBy: null),
            CancellationToken.None);

        records.Should().HaveCount(5);
        total.Should().Be(15);
        totalDisplay.Should().Be(15);
    }

    [Fact]
    public async Task Branch_filter_excludes_purchases_from_other_branches_for_non_global_user()
    {
        var otherBranch = Guid.NewGuid();
        await SeedAsync(countCurrent: 3, countOtherBranch: 4, otherBranchId: otherBranch);
        var sut = BuildHandler();

        var (records, total, _) = await sut.Handle(
            new GetPagedPurchasesQuery(PageIndex: 1, PageSize: 50, SearchText: null, SortBy: null),
            CancellationToken.None);

        total.Should().Be(3);
        records.Should().AllSatisfy(p => p.BranchId.Should().Be(BranchId));
    }
}

