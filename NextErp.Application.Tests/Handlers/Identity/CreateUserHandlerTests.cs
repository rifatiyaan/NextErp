using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NextErp.Application.Commands.Identity;
using NextErp.Application.Handlers.CommandHandlers.Identity;
using NextErp.Application.Interfaces;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Handlers.Identity;

public class CreateUserHandlerTests : HandlerTestBase
{
    // ---- helpers ----

    private static UserManager<ApplicationUser> CreateUserManager(IList<ApplicationUser> seeded)
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var manager = Substitute.For<UserManager<ApplicationUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);

        manager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(call =>
            {
                var user = call.Arg<ApplicationUser>();
                seeded.Add(user);
                return Task.FromResult(IdentityResult.Success);
            });

        manager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(Task.FromResult(IdentityResult.Success));

        manager.DeleteAsync(Arg.Any<ApplicationUser>())
            .Returns(call =>
            {
                var u = call.Arg<ApplicationUser>();
                seeded.Remove(u);
                return Task.FromResult(IdentityResult.Success);
            });

        return manager;
    }

    private static RoleManager<IdentityRole<Guid>> CreateRoleManager(IList<IdentityRole<Guid>> seeded)
    {
        var store = Substitute.For<IRoleStore<IdentityRole<Guid>>>();
        var manager = Substitute.For<RoleManager<IdentityRole<Guid>>>(
            store, null!, null!, null!, null!);

        manager.FindByNameAsync(Arg.Any<string>())
            .Returns(call =>
            {
                var n = call.Arg<string>();
                return Task.FromResult(seeded.FirstOrDefault(r =>
                    string.Equals(r.Name, n, StringComparison.OrdinalIgnoreCase)));
            });

        return manager;
    }

    private static IBranchProvider CreateBranchProvider(bool isGlobal, Guid? requiredBranchId = null)
    {
        var provider = Substitute.For<IBranchProvider>();
        provider.IsGlobal().Returns(isGlobal);
        if (!isGlobal)
        {
            provider.GetRequiredBranchId().Returns(requiredBranchId ?? Guid.NewGuid());
        }
        return provider;
    }

    // =====================================================
    //  Happy paths
    // =====================================================

    [Fact]
    public async Task CreateUser_happy_path_with_role_returns_entry_and_assigns_role()
    {
        var seededUsers = new List<ApplicationUser>();
        var roleId = Guid.NewGuid();
        var seededRoles = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid> { Id = roleId, Name = "Manager", NormalizedName = "MANAGER" }
        };
        var globalBranch = Guid.NewGuid();

        var sut = new CreateUserHandler(
            CreateUserManager(seededUsers),
            CreateRoleManager(seededRoles),
            CreateBranchProvider(isGlobal: true));

        var entry = await sut.Handle(
            new CreateUserCommand(
                Email: "alice@example.com",
                Password: "Pa$$w0rd!",
                FirstName: "Alice",
                LastName: "Smith",
                BranchId: globalBranch,
                RoleName: "Manager",
                CallerIsSuperAdmin: false,
                CallerIsGlobal: true),
            CancellationToken.None);

        entry.Should().NotBeNull();
        entry.Email.Should().Be("alice@example.com");
        entry.FirstName.Should().Be("Alice");
        entry.LastName.Should().Be("Smith");
        entry.BranchId.Should().Be(globalBranch);
        entry.RoleName.Should().Be("Manager");
        entry.RoleId.Should().Be(roleId.ToString());
        seededUsers.Should().ContainSingle(u => u.Email == "alice@example.com");
    }

    [Fact]
    public async Task CreateUser_happy_path_without_role_creates_user_with_empty_role()
    {
        var seededUsers = new List<ApplicationUser>();
        var seededRoles = new List<IdentityRole<Guid>>();
        var globalBranch = Guid.NewGuid();

        var sut = new CreateUserHandler(
            CreateUserManager(seededUsers),
            CreateRoleManager(seededRoles),
            CreateBranchProvider(isGlobal: true));

        var entry = await sut.Handle(
            new CreateUserCommand(
                Email: "bob@example.com",
                Password: "Pa$$w0rd!",
                FirstName: null,
                LastName: null,
                BranchId: globalBranch,
                RoleName: null,
                CallerIsSuperAdmin: false,
                CallerIsGlobal: true),
            CancellationToken.None);

        entry.Email.Should().Be("bob@example.com");
        entry.RoleName.Should().BeEmpty();
        entry.RoleId.Should().BeEmpty();
        seededUsers.Should().ContainSingle(u => u.Email == "bob@example.com");
    }

    // =====================================================
    //  Branch resolution
    // =====================================================

    [Fact]
    public async Task CreateUser_non_global_caller_uses_own_branch_ignoring_dto_branch()
    {
        var seededUsers = new List<ApplicationUser>();
        var seededRoles = new List<IdentityRole<Guid>>();
        var callerBranch = Guid.NewGuid();
        var attemptedBranch = Guid.NewGuid(); // different — should be overridden

        var sut = new CreateUserHandler(
            CreateUserManager(seededUsers),
            CreateRoleManager(seededRoles),
            CreateBranchProvider(isGlobal: false, requiredBranchId: callerBranch));

        var entry = await sut.Handle(
            new CreateUserCommand(
                Email: "carol@example.com",
                Password: "Pa$$w0rd!",
                FirstName: null,
                LastName: null,
                BranchId: attemptedBranch,
                RoleName: null,
                CallerIsSuperAdmin: false,
                CallerIsGlobal: false),
            CancellationToken.None);

        entry.BranchId.Should().Be(callerBranch, "non-global caller branch must override the dto value");
    }

    [Fact]
    public async Task CreateUser_global_caller_missing_branch_throws()
    {
        var seededUsers = new List<ApplicationUser>();
        var seededRoles = new List<IdentityRole<Guid>>();

        var sut = new CreateUserHandler(
            CreateUserManager(seededUsers),
            CreateRoleManager(seededRoles),
            CreateBranchProvider(isGlobal: true));

        var act = async () => await sut.Handle(
            new CreateUserCommand(
                Email: "dave@example.com",
                Password: "Pa$$w0rd!",
                FirstName: null,
                LastName: null,
                BranchId: null,
                RoleName: null,
                CallerIsSuperAdmin: false,
                CallerIsGlobal: true),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*BranchId is required*");
    }

    // =====================================================
    //  SuperAdmin guard
    // =====================================================

    [Fact]
    public async Task CreateUser_assigning_SuperAdmin_role_when_caller_is_not_SuperAdmin_throws()
    {
        var seededUsers = new List<ApplicationUser>();
        var seededRoles = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "SuperAdmin", NormalizedName = "SUPERADMIN" }
        };

        var sut = new CreateUserHandler(
            CreateUserManager(seededUsers),
            CreateRoleManager(seededRoles),
            CreateBranchProvider(isGlobal: true));

        var act = async () => await sut.Handle(
            new CreateUserCommand(
                Email: "eve@example.com",
                Password: "Pa$$w0rd!",
                FirstName: null,
                LastName: null,
                BranchId: Guid.NewGuid(),
                RoleName: "SuperAdmin",
                CallerIsSuperAdmin: false,
                CallerIsGlobal: true),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only SuperAdmin*");

        seededUsers.Should().BeEmpty("user must not be persisted when SuperAdmin guard fails");
    }

    // =====================================================
    //  Validation
    // =====================================================

    [Fact]
    public async Task CreateUser_missing_email_throws()
    {
        var sut = new CreateUserHandler(
            CreateUserManager(new List<ApplicationUser>()),
            CreateRoleManager(new List<IdentityRole<Guid>>()),
            CreateBranchProvider(isGlobal: true));

        var act = async () => await sut.Handle(
            new CreateUserCommand(
                Email: "  ",
                Password: "Pa$$w0rd!",
                FirstName: null,
                LastName: null,
                BranchId: Guid.NewGuid(),
                RoleName: null,
                CallerIsSuperAdmin: false,
                CallerIsGlobal: true),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Email is required*");
    }

    [Fact]
    public async Task CreateUser_unknown_role_cleans_up_user_and_throws()
    {
        var seededUsers = new List<ApplicationUser>();
        var seededRoles = new List<IdentityRole<Guid>>(); // empty: every role lookup misses

        var sut = new CreateUserHandler(
            CreateUserManager(seededUsers),
            CreateRoleManager(seededRoles),
            CreateBranchProvider(isGlobal: true));

        var act = async () => await sut.Handle(
            new CreateUserCommand(
                Email: "ghost@example.com",
                Password: "Pa$$w0rd!",
                FirstName: null,
                LastName: null,
                BranchId: Guid.NewGuid(),
                RoleName: "DoesNotExist",
                CallerIsSuperAdmin: false,
                CallerIsGlobal: true),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*does not exist*");

        seededUsers.Should().BeEmpty("orphan user must be cleaned up when role lookup fails");
    }
}
