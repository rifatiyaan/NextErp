using AutoMapper;
using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Category;

namespace NextErp.Application.Tests.Handlers.Category;

public class UpdateCategoryHandlerTests : HandlerTestBase
{
    private static readonly IMapper Mapper = BuildMapper();

    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
            c.AddMaps(typeof(NextErp.Application.ApplicationAssemblyMarker).Assembly));
        return cfg.CreateMapper();
    }

    private UpdateCategoryHandler BuildHandler() => new(Db, Mapper);

    private async Task<int> SeedCategoryAsync(string title = "Original")
    {
        // SeedData seeds Categories with Ids 1..10, so use 100+ to avoid clashes.
        var id = 100;
        Db.Categories.Add(new CategoryBuilder()
            .WithId(id).WithTitle(title).WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Happy_path_updates_fields_and_flushes_to_db()
    {
        var id = await SeedCategoryAsync("Old Title");
        var sut = BuildHandler();

        var cmd = new UpdateCategoryCommand(
            Id: id,
            Title: "New Title",
            Description: "New description",
            ParentId: null,
            IsActive: true);

        await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.Categories.AsNoTracking().FirstAsync(c => c.Id == id);
        fresh.Title.Should().Be("New Title");
        fresh.Description.Should().Be("New description");
        fresh.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Not_found_throws_KeyNotFoundException_with_id()
    {
        var sut = BuildHandler();

        var cmd = new UpdateCategoryCommand(
            Id: 9999,
            Title: "X",
            Description: null,
            ParentId: null);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        (await act.Should().ThrowAsync<KeyNotFoundException>())
            .WithMessage("*9999*");
    }

    [Fact]
    public async Task UpdatedAt_is_set_to_recent_UtcNow()
    {
        var id = await SeedCategoryAsync();
        var sut = BuildHandler();
        var before = DateTime.UtcNow;

        var cmd = new UpdateCategoryCommand(id, "T", null, null);
        await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.Categories.AsNoTracking().FirstAsync(c => c.Id == id);
        fresh.UpdatedAt.Should().NotBeNull();
        fresh.UpdatedAt!.Value.Should().BeOnOrAfter(before).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task IsActive_false_in_command_is_persisted()
    {
        // Confirms AutoMapper doesn't have a Condition clause silently swallowing the IsActive
        // field in updates — the handler-level mapper.Map(request, existing) should overwrite.
        var id = await SeedCategoryAsync();
        var sut = BuildHandler();

        var cmd = new UpdateCategoryCommand(id, "T", null, null, IsActive: false);
        await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.Categories.AsNoTracking().FirstAsync(c => c.Id == id);
        fresh.IsActive.Should().BeFalse();
    }
}

