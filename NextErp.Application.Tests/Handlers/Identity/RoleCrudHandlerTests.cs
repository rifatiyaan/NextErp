using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NextErp.Application.Commands.Identity;
using NextErp.Application.Handlers.CommandHandlers.Identity;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Handlers.Identity;

public class RoleCrudHandlerTests : HandlerTestBase
{
    // ---- helpers ----

    private static RoleManager<IdentityRole<Guid>> CreateRoleManager(
        IList<IdentityRole<Guid>> seeded)
    {
        var store = Substitute.For<IRoleStore<IdentityRole<Guid>>>();
        var manager = Substitute.For<RoleManager<IdentityRole<Guid>>>(
            store, null!, null!, null!, null!);

        manager.RoleExistsAsync(Arg.Any<string>())
            .Returns(call =>
            {
                var n = call.Arg<string>();
                return Task.FromResult(seeded.Any(r =>
                    string.Equals(r.Name, n, StringComparison.OrdinalIgnoreCase)));
            });

        manager.FindByIdAsync(Arg.Any<string>())
            .Returns(call =>
            {
                var idStr = call.Arg<string>();
                if (!Guid.TryParse(idStr, out var id))
                    return Task.FromResult<IdentityRole<Guid>?>(null);
                return Task.FromResult(seeded.FirstOrDefault(r => r.Id == id));
            });

        manager.FindByNameAsync(Arg.Any<string>())
            .Returns(call =>
            {
                var n = call.Arg<string>();
                return Task.FromResult(seeded.FirstOrDefault(r =>
                    string.Equals(r.Name, n, StringComparison.OrdinalIgnoreCase)));
            });

        manager.CreateAsync(Arg.Any<IdentityRole<Guid>>())
            .Returns(call =>
            {
                var role = call.Arg<IdentityRole<Guid>>();
                seeded.Add(role);
                return Task.FromResult(IdentityResult.Success);
            });

        manager.SetRoleNameAsync(Arg.Any<IdentityRole<Guid>>(), Arg.Any<string>())
            .Returns(call =>
            {
                var role = call.Arg<IdentityRole<Guid>>();
                var name = call.Arg<string>();
                role.Name = name;
                role.NormalizedName = name.ToUpperInvariant();
                return Task.FromResult(IdentityResult.Success);
            });

        manager.UpdateAsync(Arg.Any<IdentityRole<Guid>>())
            .Returns(Task.FromResult(IdentityResult.Success));

        manager.DeleteAsync(Arg.Any<IdentityRole<Guid>>())
            .Returns(call =>
            {
                var role = call.Arg<IdentityRole<Guid>>();
                seeded.Remove(role);
                return Task.FromResult(IdentityResult.Success);
            });

        return manager;
    }

    private static UserManager<ApplicationUser> CreateUserManager(
        Dictionary<string, IList<ApplicationUser>> roleToUsers)
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var manager = Substitute.For<UserManager<ApplicationUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);

        manager.GetUsersInRoleAsync(Arg.Any<string>())
            .Returns(call =>
            {
                var role = call.Arg<string>();
                if (roleToUsers.TryGetValue(role, out var users))
                    return Task.FromResult(users);
                return Task.FromResult<IList<ApplicationUser>>(new List<ApplicationUser>());
            });

        return manager;
    }

    // =====================================================
    //  CreateRole
    // =====================================================

    [Fact]
    public async Task CreateRole_happy_path_returns_entry_with_zero_users_and_no_permissions()
    {
        var seeded = new List<IdentityRole<Guid>>();
        var sut = new CreateRoleHandler(CreateRoleManager(seeded));

        var entry = await sut.Handle(new CreateRoleCommand("Manager", null), CancellationToken.None);

        entry.Should().NotBeNull();
        entry.Name.Should().Be("Manager");
        entry.UserCount.Should().Be(0);
        entry.Permissions.Should().BeEmpty();
        entry.PermissionSummary.Should().Be("No permissions");
        seeded.Should().ContainSingle(r => r.Name == "Manager");
    }

    [Fact]
    public async Task CreateRole_duplicate_name_throws()
    {
        var seeded = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Manager", NormalizedName = "MANAGER" }
        };
        var sut = new CreateRoleHandler(CreateRoleManager(seeded));

        var act = async () => await sut.Handle(new CreateRoleCommand("manager"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    // =====================================================
    //  RenameRole
    // =====================================================

    [Fact]
    public async Task RenameRole_happy_path_updates_role_name()
    {
        var roleId = Guid.NewGuid();
        var seeded = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid> { Id = roleId, Name = "Mgr", NormalizedName = "MGR" }
        };
        var sut = new RenameRoleHandler(CreateRoleManager(seeded));

        var ok = await sut.Handle(new RenameRoleCommand(roleId, "Manager"), CancellationToken.None);

        ok.Should().BeTrue();
        seeded[0].Name.Should().Be("Manager");
    }

    [Fact]
    public async Task RenameRole_not_found_returns_false()
    {
        var seeded = new List<IdentityRole<Guid>>();
        var sut = new RenameRoleHandler(CreateRoleManager(seeded));

        var ok = await sut.Handle(new RenameRoleCommand(Guid.NewGuid(), "Manager"), CancellationToken.None);

        ok.Should().BeFalse();
    }

    [Fact]
    public async Task RenameRole_superadmin_source_throws()
    {
        var roleId = Guid.NewGuid();
        var seeded = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid> { Id = roleId, Name = "SuperAdmin", NormalizedName = "SUPERADMIN" }
        };
        var sut = new RenameRoleHandler(CreateRoleManager(seeded));

        var act = async () => await sut.Handle(new RenameRoleCommand(roleId, "Boss"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*SuperAdmin*cannot be renamed*");
    }

    [Fact]
    public async Task RenameRole_to_existing_name_throws()
    {
        var roleId = Guid.NewGuid();
        var seeded = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid> { Id = roleId, Name = "Mgr", NormalizedName = "MGR" },
            new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Manager", NormalizedName = "MANAGER" }
        };
        var sut = new RenameRoleHandler(CreateRoleManager(seeded));

        var act = async () => await sut.Handle(new RenameRoleCommand(roleId, "Manager"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    // =====================================================
    //  DeleteRole
    // =====================================================

    [Fact]
    public async Task DeleteRole_happy_path_removes_role_and_clears_permissions()
    {
        var roleId = Guid.NewGuid();
        var seeded = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid> { Id = roleId, Name = "Editor", NormalizedName = "EDITOR" }
        };

        Db.RolePermissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionKey = "module.view",
            CreatedAt = DateTime.UtcNow
        });
        Db.RolePermissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionKey = "module.edit",
            CreatedAt = DateTime.UtcNow
        });
        await Db.SaveChangesAsync();

        var sut = new DeleteRoleHandler(
            Db,
            CreateUserManager(new Dictionary<string, IList<ApplicationUser>>()),
            CreateRoleManager(seeded));

        var ok = await sut.Handle(new DeleteRoleCommand(roleId), CancellationToken.None);

        ok.Should().BeTrue();
        seeded.Should().BeEmpty();
        Db.RolePermissions.Where(rp => rp.RoleId == roleId).Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteRole_not_found_returns_false()
    {
        var seeded = new List<IdentityRole<Guid>>();
        var sut = new DeleteRoleHandler(
            Db,
            CreateUserManager(new Dictionary<string, IList<ApplicationUser>>()),
            CreateRoleManager(seeded));

        var ok = await sut.Handle(new DeleteRoleCommand(Guid.NewGuid()), CancellationToken.None);

        ok.Should().BeFalse();
    }

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("Admin")]
    public async Task DeleteRole_protected_role_throws(string roleName)
    {
        var roleId = Guid.NewGuid();
        var seeded = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid> { Id = roleId, Name = roleName, NormalizedName = roleName.ToUpperInvariant() }
        };
        var sut = new DeleteRoleHandler(
            Db,
            CreateUserManager(new Dictionary<string, IList<ApplicationUser>>()),
            CreateRoleManager(seeded));

        var act = async () => await sut.Handle(new DeleteRoleCommand(roleId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*system role*");
    }

    [Fact]
    public async Task DeleteRole_with_users_throws()
    {
        var roleId = Guid.NewGuid();
        var seeded = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid> { Id = roleId, Name = "Editor", NormalizedName = "EDITOR" }
        };

        var roleToUsers = new Dictionary<string, IList<ApplicationUser>>
        {
            ["Editor"] = new List<ApplicationUser>
            {
                new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice" },
                new ApplicationUser { Id = Guid.NewGuid(), UserName = "bob" }
            }
        };

        var sut = new DeleteRoleHandler(
            Db,
            CreateUserManager(roleToUsers),
            CreateRoleManager(seeded));

        var act = async () => await sut.Handle(new DeleteRoleCommand(roleId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot delete role with 2*");
    }

    // =====================================================
    //  SetRolePermissions SuperAdmin guard (P0.3 backend)
    // =====================================================

    [Fact]
    public async Task SetRolePermissions_superadmin_role_throws()
    {
        var roleId = Guid.NewGuid();
        Db.Roles.Add(new IdentityRole<Guid>
        {
            Id = roleId,
            Name = "SuperAdmin",
            NormalizedName = "SUPERADMIN"
        });
        await Db.SaveChangesAsync();

        var seeded = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid> { Id = roleId, Name = "SuperAdmin", NormalizedName = "SUPERADMIN" }
        };

        // RoleManager mock here actually queries against Db.Roles in handler — we need a
        // RoleManager whose Roles property returns the in-memory db. Build that:
        var store = Substitute.For<IRoleStore<IdentityRole<Guid>>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole<Guid>>>(
            store, null!, null!, null!, null!);
        roleManager.Roles.Returns(_ => Db.Roles);

        var sut = new SetRolePermissionsHandler(Db, roleManager);

        var act = async () => await sut.Handle(
            new SetRolePermissionsCommand(roleId, new List<string> { "module.view" }),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*SuperAdmin*bypass*");
    }
}
