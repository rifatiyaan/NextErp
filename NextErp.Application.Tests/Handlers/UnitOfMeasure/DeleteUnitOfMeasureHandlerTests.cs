using NextErp.Application.Commands.UnitOfMeasure;
using NextErp.Application.Handlers.CommandHandlers.UnitOfMeasure;

namespace NextErp.Application.Tests.Handlers.UnitOfMeasure;

public class DeleteUnitOfMeasureHandlerTests : HandlerTestBase
{
    private DeleteUnitOfMeasureHandler BuildHandler() => new(Db);

    private async Task<int> SeedAsync(bool isSystem = false)
    {
        var id = 100;
        var b = new UnitOfMeasureBuilder().WithId(id).WithName("Carton").WithAbbreviation("ctn");
        if (isSystem) b = b.AsSystem();
        Db.UnitOfMeasures.Add(b.Build());
        await Db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Soft_delete_keeps_row_and_flips_IsActive_to_false()
    {
        var id = await SeedAsync();
        var before = DateTime.UtcNow;
        var sut = BuildHandler();

        await sut.Handle(new DeleteUnitOfMeasureCommand(id), CancellationToken.None);

        var stillExists = await Db.UnitOfMeasures.AsNoTracking().AnyAsync(u => u.Id == id);
        stillExists.Should().BeTrue();

        var fresh = await Db.UnitOfMeasures.AsNoTracking().FirstAsync(u => u.Id == id);
        fresh.IsActive.Should().BeFalse();
        fresh.UpdatedAt.Should().NotBeNull();
        fresh.UpdatedAt!.Value.Should().BeOnOrAfter(before).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task System_unit_cannot_be_deleted()
    {
        var id = await SeedAsync(isSystem: true);
        var sut = BuildHandler();

        var act = async () => await sut.Handle(new DeleteUnitOfMeasureCommand(id), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*System units*");

        // Row remains untouched.
        var fresh = await Db.UnitOfMeasures.AsNoTracking().FirstAsync(u => u.Id == id);
        fresh.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Not_found_throws_InvalidOperationException_with_id()
    {
        var sut = BuildHandler();

        var act = async () => await sut.Handle(new DeleteUnitOfMeasureCommand(99999), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*99999*");
    }
}

