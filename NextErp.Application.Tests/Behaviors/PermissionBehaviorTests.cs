using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using NextErp.Application.Commands.Identity;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Behaviors;
using NextErp.Application.Common.Caching;
using NextErp.Application.Common.Exceptions;
using NextErp.Application.Handlers.CommandHandlers.Identity;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Behaviors;

public class PermissionBehaviorTests : HandlerTestBase
{
    [RequiresPermission("Test.Allowed")]
    public record GuardedCommand : IRequest<Unit>;

    public record OpenCommand : IRequest<Unit>;

    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly PermissionCacheSignal _signal = new();

    private PermissionBehavior<GuardedCommand, Unit> BuildGuarded(IUserContext user) =>
        new(user, Db, Substitute.For<IServiceProvider>(), _cache, _signal);

    private static RequestHandlerDelegate<Unit> NextReturning(Action? sideEffect = null) =>
        ct =>
        {
            sideEffect?.Invoke();
            return Task.FromResult(Unit.Value);
        };

    private static IUserContext BuildUser(
        bool authenticated = true,
        bool superAdmin = false,
        Guid? roleId = null)
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsAuthenticated.Returns(authenticated);
        ctx.IsSuperAdmin.Returns(superAdmin);
        ctx.PrimaryRoleId.Returns(roleId);
        return ctx;
    }

    private async Task SeedGrantAsync(Guid roleId, string key)
    {
        Db.RolePermissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionKey = key,
            CreatedAt = DateTime.UtcNow
        });
        await Db.SaveChangesAsync();
    }

    [Fact]
    public async Task No_attribute_skips_check_and_invokes_next()
    {
        var user = BuildUser();
        var sut = new PermissionBehavior<OpenCommand, Unit>(
            user, Db, Substitute.For<IServiceProvider>(), _cache, _signal);

        var nextCalls = 0;
        var result = await sut.Handle(new OpenCommand(), NextReturning(() => nextCalls++), CancellationToken.None);

        result.Should().Be(Unit.Value);
        nextCalls.Should().Be(1);
    }

    [Fact]
    public async Task Has_attribute_and_user_has_permission_invokes_next()
    {
        var roleId = Guid.NewGuid();
        await SeedGrantAsync(roleId, "Test.Allowed");

        var sut = BuildGuarded(BuildUser(roleId: roleId));

        var nextCalls = 0;
        var result = await sut.Handle(new GuardedCommand(), NextReturning(() => nextCalls++), CancellationToken.None);

        result.Should().Be(Unit.Value);
        nextCalls.Should().Be(1);
    }

    [Fact]
    public async Task Has_attribute_missing_permission_throws_Forbidden()
    {
        var roleId = Guid.NewGuid();
        // Seed an unrelated permission row so the table isn't empty but our key isn't present.
        await SeedGrantAsync(roleId, "Some.OtherPermission");

        var sut = BuildGuarded(BuildUser(roleId: roleId));

        var act = async () => await sut.Handle(new GuardedCommand(), NextReturning(), CancellationToken.None);

        (await act.Should().ThrowAsync<ForbiddenAccessException>())
            .WithMessage("*Test.Allowed*");
    }

    [Fact]
    public async Task SuperAdmin_bypasses_permission_check()
    {
        // No matching RolePermission row exists; super-admin should still pass.
        var sut = BuildGuarded(BuildUser(superAdmin: true));

        var nextCalls = 0;
        var result = await sut.Handle(new GuardedCommand(), NextReturning(() => nextCalls++), CancellationToken.None);

        result.Should().Be(Unit.Value);
        nextCalls.Should().Be(1);
    }

    [Fact]
    public async Task Second_call_is_served_from_cache()
    {
        var roleId = Guid.NewGuid();
        await SeedGrantAsync(roleId, "Test.Allowed");

        var sut = BuildGuarded(BuildUser(roleId: roleId));
        await sut.Handle(new GuardedCommand(), NextReturning(), CancellationToken.None);

        // Remove the grant directly (no command handler -> no invalidation).
        // The cached set must still grant access.
        Db.RolePermissions.RemoveRange(Db.RolePermissions);
        await Db.SaveChangesAsync();

        var nextCalls = 0;
        await sut.Handle(new GuardedCommand(), NextReturning(() => nextCalls++), CancellationToken.None);
        nextCalls.Should().Be(1, "the second check should be served from cache, not re-queried");
    }

    [Fact]
    public async Task Revoking_permissions_via_handler_invalidates_the_cache()
    {
        var roleId = Guid.NewGuid();
        Db.Roles.Add(new IdentityRole<Guid>
        {
            Id = roleId,
            Name = "Clerk",
            NormalizedName = "CLERK",
        });
        await Db.SaveChangesAsync();
        await SeedGrantAsync(roleId, "Test.Allowed");

        var sut = BuildGuarded(BuildUser(roleId: roleId));
        await sut.Handle(new GuardedCommand(), NextReturning(), CancellationToken.None); // warm cache

        // Revoke every permission through the command handler, which must invalidate.
        var store = Substitute.For<IRoleStore<IdentityRole<Guid>>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole<Guid>>>(store, null!, null!, null!, null!);
        roleManager.Roles.Returns(_ => Db.Roles);
        var revoke = new SetRolePermissionsHandler(Db, roleManager, _signal);
        (await revoke.Handle(new SetRolePermissionsCommand(roleId, new List<string>()), CancellationToken.None))
            .Should().BeTrue();

        var act = async () => await sut.Handle(new GuardedCommand(), NextReturning(), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenAccessException>(
            "revoking permissions must drop the cached set immediately");
    }
}
