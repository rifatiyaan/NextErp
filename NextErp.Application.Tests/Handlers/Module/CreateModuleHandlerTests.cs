using AutoMapper;
using NextErp.Application.Commands.Module;
using NextErp.Application.DTOs;
using NextErp.Application.Handlers.CommandHandlers.Module;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Module;

public class CreateModuleHandlerTests : HandlerTestBase
{
    private static readonly IMapper Mapper = BuildMapper();

    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
            c.AddMaps(typeof(NextErp.Application.ApplicationAssemblyMarker).Assembly));
        return cfg.CreateMapper();
    }

    private CreateModuleHandler BuildHandler() => new(Db, Mapper);

    [Fact]
    public async Task Happy_path_creates_top_level_module_and_returns_id()
    {
        var sut = BuildHandler();

        var cmd = new CreateModuleCommand(new DTOs.Module.Request.Create.Single
        {
            Title = "Inventory",
            Type = ModuleType.Module,
            Url = "/inventory",
            Order = 1
        });

        var id = await sut.Handle(cmd, CancellationToken.None);

        id.Should().BeGreaterThan(0);

        var fresh = await Db.Modules.AsNoTracking().FirstAsync(m => m.Id == id);
        fresh.Title.Should().Be("Inventory");
        fresh.Type.Should().Be(ModuleType.Module);
        fresh.ParentId.Should().BeNull();
        fresh.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task With_ParentId_child_module_attaches_to_parent()
    {
        // Seed a parent module first.
        const int parentId = 100;
        Db.Modules.Add(new ModuleBuilder()
            .WithId(parentId).WithTitle("Parent").WithType(ModuleType.Module).Build());
        await Db.SaveChangesAsync();

        var sut = BuildHandler();
        var cmd = new CreateModuleCommand(new DTOs.Module.Request.Create.Single
        {
            Title = "Child",
            Type = ModuleType.Link,
            ParentId = parentId,
            Url = "/parent/child",
            Order = 1
        });

        var id = await sut.Handle(cmd, CancellationToken.None);

        var fresh = await Db.Modules.AsNoTracking().FirstAsync(m => m.Id == id);
        fresh.ParentId.Should().Be(parentId);
        fresh.Title.Should().Be("Child");
        fresh.Type.Should().Be(ModuleType.Link);
    }
}

