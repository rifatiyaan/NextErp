using NextErp.Application.Interfaces;
using NextErp.Application.Services;
using NextErp.Infrastructure;
using NSubstitute;

namespace NextErp.Application.Tests.Infrastructure;

public abstract class HandlerTestBase : IDisposable
{
    protected readonly Guid TenantId = Guid.NewGuid();
    protected readonly Guid BranchId = Guid.NewGuid();
    protected readonly Guid UserId = Guid.NewGuid();
    protected readonly IBranchProvider BranchProvider;
    protected readonly IUserContext UserContext;
    protected readonly INotificationService Notifications;

    private readonly TestDbContextFactory.TestContext _ctx;
    protected readonly ApplicationDbContext Db;

    protected HandlerTestBase()
    {
        BranchProvider = Substitute.For<IBranchProvider>();
        ConfigureBranchProvider(BranchProvider);

        UserContext = Substitute.For<IUserContext>();
        ConfigureUserContext(UserContext);

        _ctx = TestDbContextFactory.Create(BranchProvider);
        Db = _ctx.Db;

        // Real NotificationService over the in-memory DbContext — tests can assert on
        // staged Notifications via Db.Notifications without setting up extra fakes.
        Notifications = new NotificationService(Db, BranchProvider, UserContext);
    }

    protected virtual void ConfigureBranchProvider(IBranchProvider provider)
    {
        provider.GetBranchId().Returns(BranchId);
        provider.GetRequiredBranchId().Returns(BranchId);
        provider.IsGlobal().Returns(false);
    }

    protected virtual void ConfigureUserContext(IUserContext context)
    {
        context.IsAuthenticated.Returns(true);
        context.UserId.Returns(UserId);
        context.IsSuperAdmin.Returns(false);
    }

    public void Dispose() => _ctx.Dispose();
}

