using NextErp.Application.Commands.UnitOfMeasure;
using NextErp.Application.Handlers.CommandHandlers.UnitOfMeasure;

namespace NextErp.Application.Tests.Handlers.UnitOfMeasure;

public class UpdateUnitOfMeasureHandlerTests : HandlerTestBase
{
    private UpdateUnitOfMeasureHandler BuildHandler() => new(Db);

    private async Task<int> SeedAsync(bool isSystem = false, string name = "Carton")
    {
        var id = 100;
        var b = new UnitOfMeasureBuilder()
            .WithId(id).WithName(name).WithAbbreviation("ctn").WithCategory("Count");
        if (isSystem) b = b.AsSystem();
        Db.UnitOfMeasures.Add(b.Build());
        await Db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Happy_path_updates_name_abbreviation_category_isactive()
    {
        var id = await SeedAsync();
        var sut = BuildHandler();

        var cmd = new UpdateUnitOfMeasureCommand(
            Id: id,
            Name: "Big Carton",
            Abbreviation: "bctn",
            Category: "Volume",
            IsActive: true);

        await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.UnitOfMeasures.AsNoTracking().FirstAsync(u => u.Id == id);
        fresh.Name.Should().Be("Big Carton");
        // Handler also pushes Name into Title for back-compat.
        fresh.Title.Should().Be("Big Carton");
        fresh.Abbreviation.Should().Be("bctn");
        fresh.Category.Should().Be("Volume");
        fresh.IsActive.Should().BeTrue();
        fresh.UpdatedAt.Should().NotBeNull();
        fresh.UpdatedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Not_found_throws_InvalidOperationException_with_id()
    {
        var sut = BuildHandler();

        var cmd = new UpdateUnitOfMeasureCommand(
            Id: 99999,
            Name: "X", Abbreviation: "x", Category: null, IsActive: true);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*99999*");
    }

    [Fact]
    public async Task System_unit_can_be_updated_without_special_restriction()
    {
        // The handler does not enforce IsSystem-protection on update — only on delete.
        var id = await SeedAsync(isSystem: true, name: "SystemUnit");
        var sut = BuildHandler();

        var cmd = new UpdateUnitOfMeasureCommand(
            Id: id,
            Name: "RenamedSystemUnit",
            Abbreviation: "rsu",
            Category: "Count",
            IsActive: true);

        await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.UnitOfMeasures.AsNoTracking().FirstAsync(u => u.Id == id);
        fresh.Name.Should().Be("RenamedSystemUnit");
        fresh.Abbreviation.Should().Be("rsu");
        fresh.IsSystem.Should().BeTrue(); // IsSystem flag preserved
    }
}

