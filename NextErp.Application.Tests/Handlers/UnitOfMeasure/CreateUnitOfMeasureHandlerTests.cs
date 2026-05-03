using NextErp.Application.Commands.UnitOfMeasure;
using NextErp.Application.Handlers.CommandHandlers.UnitOfMeasure;

namespace NextErp.Application.Tests.Handlers.UnitOfMeasure;

public class CreateUnitOfMeasureHandlerTests : HandlerTestBase
{
    private CreateUnitOfMeasureHandler BuildHandler() => new(Db);

    [Fact]
    public async Task Happy_path_creates_uom_and_returns_dto_with_id()
    {
        var sut = BuildHandler();

        // Use unique abbreviations not in SeedData (which seeds common units with Ids 1-10).
        var cmd = new CreateUnitOfMeasureCommand(
            Name: "TestUnitX",
            Abbreviation: "txu");

        var dto = await sut.Handle(cmd, CancellationToken.None);

        dto.Id.Should().BeGreaterThan(0);
        dto.Name.Should().Be("TestUnitX");
        dto.Abbreviation.Should().Be("txu");
        dto.Title.Should().Be("TestUnitX");
        dto.IsActive.Should().BeTrue();
        dto.IsSystem.Should().BeFalse();

        var fresh = await Db.UnitOfMeasures.AsNoTracking().FirstAsync(u => u.Id == dto.Id);
        fresh.Name.Should().Be("TestUnitX");
        fresh.Abbreviation.Should().Be("txu");
    }

    [Fact]
    public async Task With_category_and_IsSystem_false_fields_are_preserved()
    {
        var sut = BuildHandler();

        var cmd = new CreateUnitOfMeasureCommand(
            Name: "TestUnitY",
            Abbreviation: "tyu",
            Category: "Length",
            IsSystem: false);

        var dto = await sut.Handle(cmd, CancellationToken.None);

        dto.Category.Should().Be("Length");
        dto.IsSystem.Should().BeFalse();

        var fresh = await Db.UnitOfMeasures.AsNoTracking().FirstAsync(u => u.Id == dto.Id);
        fresh.Category.Should().Be("Length");
        fresh.IsSystem.Should().BeFalse();
    }
}

