using NextErp.Application.Handlers.QueryHandlers.Category;
using NextErp.Application.Queries;

namespace NextErp.Application.Tests.Handlers.Category;

public class GetPagedCategoriesHandlerTests : HandlerTestBase
{
    private GetPagedCategoriesHandler BuildHandler() => new(Db);

    private async Task SeedAsync(string titlePrefix, int count, int idOffset = 100)
    {
        for (var i = 0; i < count; i++)
        {
            Db.Categories.Add(new CategoryBuilder()
                .WithId(idOffset + i)
                .WithTitle($"{titlePrefix} {i:D2}")
                .WithTenant(TenantId)
                .WithBranch(BranchId)
                .Build());
        }
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Pagination_with_search_filter_page_2_size_5_returns_5_rows_and_total_15()
    {
        // 15 rows with prefix "ZZZTest" — search isolates them from seeded rows.
        await SeedAsync("ZZZTest", count: 15);
        var sut = BuildHandler();

        var result = await sut.Handle(
            new GetPagedCategoriesQuery(PageIndex: 2, PageSize: 5, SearchText: "ZZZTest", SortBy: "title"),
            CancellationToken.None);

        result.Records.Should().HaveCount(5);
        result.Total.Should().Be(15);
        result.TotalDisplay.Should().Be(15);
    }

    [Fact]
    public async Task Search_by_title_returns_only_matching_rows()
    {
        await SeedAsync("AlphaCat", count: 4, idOffset: 100);
        await SeedAsync("BetaCat", count: 7, idOffset: 200);
        var sut = BuildHandler();

        var result = await sut.Handle(
            new GetPagedCategoriesQuery(PageIndex: 1, PageSize: 50, SearchText: "BetaCat", SortBy: "title"),
            CancellationToken.None);

        result.Total.Should().Be(7);
        result.Records.Should().AllSatisfy(c => c.Title.Should().StartWith("BetaCat"));
    }
}

