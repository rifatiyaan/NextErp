using NextErp.Application.Handlers.QueryHandlers.Sale;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Sale;

public class GetPagedSalesHandlerTests : HandlerTestBase
{
    private GetPagedSalesHandler BuildHandler() => new(Db);

    private async Task SeedSalesAsync(int count = 15)
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());
        var partyId = Guid.NewGuid();
        Db.Parties.Add(new PartyBuilder()
            .WithId(partyId).WithTitle("Acme Industries").WithTenant(TenantId).WithBranch(BranchId).Build());

        for (var i = 0; i < count; i++)
        {
            Db.Sales.Add(new SaleBuilder()
                .WithSaleNumber($"S-{i:D3}")
                .WithTitle($"Sale {i}")
                .WithParty(partyId)
                .WithSaleDate(new DateTime(2026, 1, 1).AddDays(i))
                .WithTotalAmount(100m + i)
                .WithFinalAmount(100m + i)
                .WithTenant(TenantId)
                .WithBranch(BranchId)
                .Build());
        }
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Pagination_page_2_size_5_returns_5_rows_and_total_15()
    {
        await SeedSalesAsync(count: 15);
        var sut = BuildHandler();

        var result = await sut.Handle(
            new GetPagedSalesQuery(PageIndex: 2, PageSize: 5, SearchText: null, SortBy: null),
            CancellationToken.None);

        result.Records.Should().HaveCount(5);
        result.Total.Should().Be(15);
        result.TotalDisplay.Should().Be(15);
    }

    [Fact]
    public async Task Search_filter_matches_SaleNumber_substring()
    {
        await SeedSalesAsync(count: 15);
        var sut = BuildHandler();

        // SaleNumbers go S-000..S-014, so "S-01" matches S-010..S-014 (5 rows).
        var result = await sut.Handle(
            new GetPagedSalesQuery(PageIndex: 1, PageSize: 50, SearchText: "S-01", SortBy: null),
            CancellationToken.None);

        result.Total.Should().Be(5);
        result.Records.Should().AllSatisfy(r => r.SaleNumber.Should().StartWith("S-01"));
    }
}

