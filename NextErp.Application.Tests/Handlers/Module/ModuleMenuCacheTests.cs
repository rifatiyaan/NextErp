using Microsoft.Extensions.Caching.Memory;
using NextErp.Application.Commands.Module;
using NextErp.Application.Common.Caching;
using NextErp.Application.DTOs;
using NextErp.Application.Handlers.CommandHandlers.Module;
using NextErp.Application.Handlers.QueryHandlers.Module;
using NextErp.Application.Queries.Module;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Handlers.Module;

public class ModuleMenuCacheTests : HandlerTestBase
{
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly ModuleCacheSignal _signal = new();

    private GetMenuByUserHandler BuildMenuHandler() => new(Db, _cache, _signal);

    private async Task SeedModuleAsync(string title, int order)
    {
        Db.Modules.Add(new ModuleBuilder()
            .WithTitle(title).WithType(ModuleType.Module).WithOrder(order)
            .WithTenant(Guid.Empty).Build());
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Menu_is_served_from_cache_on_second_call()
    {
        await SeedModuleAsync("Inventory", 1);
        var menu = BuildMenuHandler();

        var first = await menu.Handle(new GetMenuByUserQuery(UserId, Guid.Empty), CancellationToken.None);
        first.Should().HaveCount(1);

        // Insert directly (no command handler -> no invalidation). The cache must hide it.
        await SeedModuleAsync("Sales", 2);

        var second = await menu.Handle(new GetMenuByUserQuery(UserId, Guid.Empty), CancellationToken.None);
        second.Should().HaveCount(1, "the second call should be served from cache, not re-queried");
    }

    [Fact]
    public async Task Creating_a_module_invalidates_the_menu_cache()
    {
        await SeedModuleAsync("Inventory", 1);
        var menu = BuildMenuHandler();

        var first = await menu.Handle(new GetMenuByUserQuery(UserId, Guid.Empty), CancellationToken.None);
        first.Should().HaveCount(1);

        // Create via the command handler, which must invalidate the menu cache.
        var create = new CreateModuleHandler(Db, _signal);
        await create.Handle(new CreateModuleCommand(new DTOs.Module.CreateModuleRequest
        {
            Title = "Sales",
            Type = ModuleType.Module,
            Url = "/sales",
            Order = 2,
        }), CancellationToken.None);

        var second = await menu.Handle(new GetMenuByUserQuery(UserId, Guid.Empty), CancellationToken.None);
        second.Should().HaveCount(2, "creating a module should drop the cached menu so the new one appears");
    }
}
