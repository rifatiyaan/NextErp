using AutoMapper;
using NextErp.Application.Commands;
using NextErp.Application.Handlers.CommandHandlers.Category;

namespace NextErp.Application.Tests.Handlers.Category;

public class CreateCategoryHandlerTests : HandlerTestBase
{
    private static readonly IMapper Mapper = BuildMapper();

    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
            c.AddMaps(typeof(NextErp.Application.ApplicationAssemblyMarker).Assembly));
        return cfg.CreateMapper();
    }

    private CreateCategoryHandler BuildHandler() => new(Db, Mapper);

    [Fact]
    public async Task Happy_path_creates_top_level_category_and_returns_id()
    {
        var sut = BuildHandler();

        var cmd = new CreateCategoryCommand(
            Title: "Office Supplies",
            Description: "Stationery and similar",
            ParentId: null);

        var id = await sut.Handle(cmd, CancellationToken.None);

        id.Should().BeGreaterThan(0);

        var fresh = await Db.Categories.AsNoTracking().FirstAsync(c => c.Id == id);
        fresh.Title.Should().Be("Office Supplies");
        fresh.Description.Should().Be("Stationery and similar");
        fresh.ParentId.Should().BeNull();
        fresh.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task With_parent_seeded_child_is_created_with_ParentId()
    {
        // SeedData populates Categories 1..10; use 100+ to avoid clashes.
        const int parentId = 100;
        Db.Categories.Add(new CategoryBuilder()
            .WithId(parentId).WithTitle("Parent").WithTenant(TenantId).WithBranch(BranchId).Build());
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var cmd = new CreateCategoryCommand(
            Title: "Child",
            Description: null,
            ParentId: parentId);

        var id = await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.Categories.AsNoTracking().FirstAsync(c => c.Id == id);
        fresh.ParentId.Should().Be(parentId);
        fresh.Title.Should().Be("Child");
    }

    [Fact]
    public async Task CreatedAt_is_set_to_recent_UtcNow()
    {
        var before = DateTime.UtcNow;
        var sut = BuildHandler();

        var cmd = new CreateCategoryCommand("Timed", null, null);
        var id = await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.Categories.AsNoTracking().FirstAsync(c => c.Id == id);
        fresh.CreatedAt.Should().BeOnOrAfter(before).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}

