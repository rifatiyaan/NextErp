using NextErp.Application.Handlers.QueryHandlers.UnitOfMeasure;
using NextErp.Application.Queries;

namespace NextErp.Application.Tests.Handlers.UnitOfMeasure;

public class GetAllUnitOfMeasuresHandlerTests : HandlerTestBase
{
    private GetAllUnitOfMeasuresHandler BuildHandler() => new(Db);

    [Fact]
    public async Task All_seeded_uoms_returned_ordered_by_name()
    {
        // SeedData seeds common units (Ids 1-10). We assert the list is non-empty and
        // ordered by Name ascending — handler-side OrderBy(u => u.Name).
        var sut = BuildHandler();
        var result = await sut.Handle(new GetAllUnitOfMeasuresQuery(), CancellationToken.None);

        result.Count.Should().BeGreaterThan(0);
        result.Select(u => u.Name).Should().BeInAscendingOrder();
    }
}

