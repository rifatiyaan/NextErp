using NextErp.Application.Commands.Module;
using NextErp.Application.Handlers.CommandHandlers.Module;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Module;

public class DeleteModuleHandlerTests : HandlerTestBase
{
    private DeleteModuleHandler BuildHandler() => new(Db);

    private async Task<int> SeedModuleAsync()
    {
        var id = 100;
        Db.Modules.Add(new ModuleBuilder()
            .WithId(id).WithTitle("Soft Delete Me").WithType(ModuleType.Link)
            .Build());
        await Db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Soft_delete_keeps_row_and_flips_IsActive_to_false()
    {
        var id = await SeedModuleAsync();
        var before = DateTime.UtcNow;
        var sut = BuildHandler();

        await sut.Handle(new DeleteModuleCommand(id), CancellationToken.None);

        // Row must still exist (soft delete, not hard delete).
        var stillExists = await Db.Modules.AsNoTracking().AnyAsync(m => m.Id == id);
        stillExists.Should().BeTrue();

        var fresh = await Db.Modules.AsNoTracking().FirstAsync(m => m.Id == id);
        fresh.IsActive.Should().BeFalse();
        fresh.UpdatedAt.Should().NotBeNull();
        fresh.UpdatedAt!.Value.Should().BeOnOrAfter(before).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Soft_deleted_module_excluded_from_active_filter()
    {
        var id = await SeedModuleAsync();
        var sut = BuildHandler();

        await sut.Handle(new DeleteModuleCommand(id), CancellationToken.None);

        // Module has no global IsActive query filter; consumers must filter explicitly.
        var activeOnly = await Db.Modules.AsNoTracking()
            .Where(m => m.IsActive && m.Id == id)
            .ToListAsync();
        activeOnly.Should().BeEmpty();
    }

    [Fact]
    public async Task Not_found_throws_KeyNotFoundException()
    {
        var sut = BuildHandler();

        var act = async () => await sut.Handle(new DeleteModuleCommand(9999), CancellationToken.None);

        (await act.Should().ThrowAsync<KeyNotFoundException>())
            .WithMessage("*9999*");
    }
}

