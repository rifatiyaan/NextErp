using MediatR;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Behaviors;
using NextErp.Application.Common.Exceptions;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Behaviors;

public class PermissionBehaviorTests : HandlerTestBase
{
    [RequiresPermission("Test.Allowed")]
    public record GuardedCommand : IRequest<Unit>;

    public record OpenCommand : IRequest<Unit>;

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

    [Fact]
    public async Task No_attribute_skips_check_and_invokes_next()
    {
        var user = BuildUser();
        var sut = new PermissionBehavior<OpenCommand, Unit>(user, Db, Substitute.For<IServiceProvider>());

        var nextCalls = 0;
        var result = await sut.Handle(new OpenCommand(), NextReturning(() => nextCalls++), CancellationToken.None);

        result.Should().Be(Unit.Value);
        nextCalls.Should().Be(1);
    }

    [Fact]
    public async Task Has_attribute_and_user_has_permission_invokes_next()
    {
        var roleId = Guid.NewGuid();
        Db.RolePermissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionKey = "Test.Allowed",
            CreatedAt = DateTime.UtcNow
        });
        await Db.SaveChangesAsync();

        var user = BuildUser(roleId: roleId);
        var sut = new PermissionBehavior<GuardedCommand, Unit>(user, Db, Substitute.For<IServiceProvider>());

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
        Db.RolePermissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionKey = "Some.OtherPermission",
            CreatedAt = DateTime.UtcNow
        });
        await Db.SaveChangesAsync();

        var user = BuildUser(roleId: roleId);
        var sut = new PermissionBehavior<GuardedCommand, Unit>(user, Db, Substitute.For<IServiceProvider>());

        var act = async () => await sut.Handle(new GuardedCommand(), NextReturning(), CancellationToken.None);

        (await act.Should().ThrowAsync<ForbiddenAccessException>())
            .WithMessage("*Test.Allowed*");
    }

    [Fact]
    public async Task SuperAdmin_bypasses_permission_check()
    {
        // No matching RolePermission row exists; super-admin should still pass.
        var user = BuildUser(superAdmin: true);
        var sut = new PermissionBehavior<GuardedCommand, Unit>(user, Db, Substitute.For<IServiceProvider>());

        var nextCalls = 0;
        var result = await sut.Handle(new GuardedCommand(), NextReturning(() => nextCalls++), CancellationToken.None);

        result.Should().Be(Unit.Value);
        nextCalls.Should().Be(1);
    }
}
