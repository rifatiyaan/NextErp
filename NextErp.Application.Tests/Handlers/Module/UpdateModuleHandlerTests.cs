using AutoMapper;
using NextErp.Application.Commands.Module;
using NextErp.Application.DTOs;
using NextErp.Application.Handlers.CommandHandlers.Module;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Module;

public class UpdateModuleHandlerTests : HandlerTestBase
{
    private static readonly IMapper Mapper = BuildMapper();

    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
            c.AddMaps(typeof(NextErp.Application.ApplicationAssemblyMarker).Assembly));
        return cfg.CreateMapper();
    }

    private UpdateModuleHandler BuildHandler() => new(Db, Mapper);

    private async Task<int> SeedModuleAsync(string title = "Old Module", string? icon = "old-icon")
    {
        var id = 100;
        Db.Modules.Add(new ModuleBuilder()
            .WithId(id).WithTitle(title).WithIcon(icon).WithType(ModuleType.Link)
            .Build());
        await Db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Happy_path_updates_title_and_icon()
    {
        var id = await SeedModuleAsync();
        var sut = BuildHandler();

        var dto = new DTOs.Module.Request.Update.Single
        {
            Id = id,
            Title = "New Module Title",
            Icon = "new-icon",
            Type = ModuleType.Link,
            Order = 5,
            IsActive = true,
        };
        var cmd = new UpdateModuleCommand(id, dto);

        await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.Modules.AsNoTracking().FirstAsync(m => m.Id == id);
        fresh.Title.Should().Be("New Module Title");
        fresh.Icon.Should().Be("new-icon");
        fresh.Order.Should().Be(5);
    }

    [Fact]
    public async Task Not_found_throws_KeyNotFoundException()
    {
        var sut = BuildHandler();

        var dto = new DTOs.Module.Request.Update.Single
        {
            Id = 9999,
            Title = "X",
            Type = ModuleType.Link,
        };
        var cmd = new UpdateModuleCommand(9999, dto);

        var act = async () => await sut.Handle(cmd, CancellationToken.None);

        (await act.Should().ThrowAsync<KeyNotFoundException>())
            .WithMessage("*9999*");
    }

    [Fact]
    public async Task UpdatedAt_is_set_to_recent_UtcNow()
    {
        var id = await SeedModuleAsync();
        var before = DateTime.UtcNow;
        var sut = BuildHandler();

        var dto = new DTOs.Module.Request.Update.Single
        {
            Id = id,
            Title = "T",
            Type = ModuleType.Link,
            IsActive = true,
        };
        await sut.Handle(new UpdateModuleCommand(id, dto), CancellationToken.None);

        var fresh = await Db.Modules.AsNoTracking().FirstAsync(m => m.Id == id);
        fresh.UpdatedAt.Should().NotBeNull();
        fresh.UpdatedAt!.Value.Should().BeOnOrAfter(before).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}

