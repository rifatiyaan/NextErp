using NextErp.Application.Handlers.QueryHandlers.UnitOfMeasure;
using NextErp.Application.Queries;

namespace NextErp.Application.Tests.Handlers.UnitOfMeasure;

public class GetUnitOfMeasureByIdHandlerTests : HandlerTestBase
{
    private GetUnitOfMeasureByIdHandler BuildHandler() => new(Db);

    [Fact]
    public async Task Existing_uom_returned_as_dto()
    {
        // SeedData populates common units with Ids 1-10. We pick the first seeded row to
        // verify the handler returns a fully-populated DTO without persistence churn.
        var seeded = await Db.UnitOfMeasures.AsNoTracking().FirstAsync(u => u.Id == 1);

        var sut = BuildHandler();
        var result = await sut.Handle(new GetUnitOfMeasureByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be(seeded.Name);
        result.Abbreviation.Should().Be(seeded.Abbreviation);
    }
}

