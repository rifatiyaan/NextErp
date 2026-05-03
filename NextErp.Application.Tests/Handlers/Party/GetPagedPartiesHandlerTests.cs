using NextErp.Application.Handlers.QueryHandlers.Party;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Party;

public class GetPagedPartiesHandlerTests : HandlerTestBase
{
    private GetPagedPartiesHandler BuildHandler() => new(Db);

    private async Task SeedAsync(int customers = 10, int suppliers = 5)
    {
        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId).Build());

        for (var i = 0; i < customers; i++)
        {
            Db.Parties.Add(new PartyBuilder()
                .WithId(Guid.NewGuid())
                .WithTitle($"Customer {i:D2}")
                .WithTenant(TenantId)
                .WithBranch(BranchId)
                .Build());
        }
        for (var i = 0; i < suppliers; i++)
        {
            Db.Parties.Add(new PartyBuilder()
                .WithId(Guid.NewGuid())
                .WithTitle($"Supplier {i:D2}")
                .AsSupplier()
                .WithTenant(TenantId)
                .WithBranch(BranchId)
                .Build());
        }
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Pagination_page_2_size_5_returns_5_rows_and_total_15()
    {
        await SeedAsync(customers: 10, suppliers: 5);
        var sut = BuildHandler();

        var (records, total, totalDisplay) = await sut.Handle(
            new GetPagedPartiesQuery(PageIndex: 2, PageSize: 5, SearchText: null, SortBy: null),
            CancellationToken.None);

        records.Should().HaveCount(5);
        total.Should().Be(15);
        totalDisplay.Should().Be(15);
    }

    [Fact]
    public async Task PartyType_filter_returns_only_suppliers()
    {
        await SeedAsync(customers: 10, suppliers: 5);
        var sut = BuildHandler();

        var (records, total, _) = await sut.Handle(
            new GetPagedPartiesQuery(PageIndex: 1, PageSize: 50, SearchText: null, SortBy: null,
                PartyType: PartyType.Supplier),
            CancellationToken.None);

        total.Should().Be(5);
        records.Should().AllSatisfy(p => p.PartyType.Should().Be(PartyType.Supplier));
    }
}

