using Microsoft.AspNetCore.Identity;
using NextErp.Application.Handlers.QueryHandlers.Identity;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Handlers.Identity;

public class GetIdentityDashboardHandlerTests : HandlerTestBase
{
    private UserManager<ApplicationUser> CreateUserManager(Dictionary<Guid, IList<string>> userRoles)
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var manager = Substitute.For<UserManager<ApplicationUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);

        manager.Users.Returns(_ => Db.Users);
        manager.GetRolesAsync(Arg.Any<ApplicationUser>())
            .Returns(call => Task.FromResult(
                userRoles.TryGetValue(call.Arg<ApplicationUser>().Id, out var rs)
                    ? rs
                    : (IList<string>)new List<string>()));

        return manager;
    }

    private RoleManager<IdentityRole<Guid>> CreateRoleManager()
    {
        var store = Substitute.For<IRoleStore<IdentityRole<Guid>>>();
        var manager = Substitute.For<RoleManager<IdentityRole<Guid>>>(
            store, null!, null!, null!, null!);
        manager.Roles.Returns(_ => Db.Roles);
        return manager;
    }

    [Fact]
    public async Task Returns_users_branches_and_role_permissions_together()
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        Db.Branches.Add(new BranchBuilder().WithId(BranchId).WithTenant(TenantId)
            .WithTitle("Main Branch").Build());
        Db.RolePermissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionKey = "Module.View",
            CreatedAt = DateTime.UtcNow,
        });
        Db.RolePermissions.Add(new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionKey = "Module.Edit",
            CreatedAt = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();

        Db.Users.Add(new ApplicationUser
        {
            Id = userId,
            UserName = "alice",
            Email = "alice@example.com",
            FirstName = "Alice",
            LastName = "Smith",
            BranchId = BranchId,
            EmailConfirmed = true,
        });
        Db.Roles.Add(new IdentityRole<Guid>
        {
            Id = roleId, Name = "Manager", NormalizedName = "MANAGER",
        });
        await Db.SaveChangesAsync();

        var userRoles = new Dictionary<Guid, IList<string>>
        {
            [userId] = new List<string> { "Manager" },
        };

        var sut = new GetIdentityDashboardHandler(
            CreateUserManager(userRoles),
            CreateRoleManager(),
            Db,
            BranchProvider);

        var dto = await sut.Handle(new GetIdentityDashboardQuery(), CancellationToken.None);

        dto.Users.Should().HaveCount(1);
        dto.Users[0].UserName.Should().Be("alice");
        dto.Users[0].RoleName.Should().Be("Manager");
        dto.Branches.Should().HaveCount(1);
        dto.Branches[0].Name.Should().Be("Main Branch");
        dto.Roles.Should().HaveCount(1);
        dto.Roles[0].Permissions.Should().BeEquivalentTo(new[] { "Module.View", "Module.Edit" });
        dto.Roles[0].UserCount.Should().Be(1);
    }

    [Fact]
    public async Task Empty_DB_returns_empty_lists_not_null()
    {
        // No branches, no users, no permissions seeded.
        var sut = new GetIdentityDashboardHandler(
            CreateUserManager(new Dictionary<Guid, IList<string>>()),
            CreateRoleManager(),
            Db,
            BranchProvider);

        var dto = await sut.Handle(new GetIdentityDashboardQuery(), CancellationToken.None);

        dto.Users.Should().NotBeNull().And.BeEmpty();
        dto.Branches.Should().NotBeNull().And.BeEmpty();
        dto.Roles.Should().NotBeNull().And.BeEmpty();
    }
}

