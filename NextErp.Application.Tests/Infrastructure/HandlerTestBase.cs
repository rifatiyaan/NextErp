using NextErp.Application.Interfaces;
using NextErp.Infrastructure;
using NSubstitute;

namespace NextErp.Application.Tests.Infrastructure;

/// <summary>
/// Base for handler tests. Spins up a SQLite in-memory <see cref="ApplicationDbContext"/>
/// scoped to a synthetic branch + tenant. Override <see cref="ConfigureBranchProvider"/>
/// for tests that need global / different branch context.
/// </summary>
public abstract class HandlerTestBase : IDisposable
{
    protected readonly Guid TenantId = Guid.NewGuid();
    protected readonly Guid BranchId = Guid.NewGuid();
    protected readonly IBranchProvider BranchProvider;
    protected readonly ApplicationDbContext Db;

    private readonly TestDbContextFactory.TestContext _ctx;

    protected HandlerTestBase()
    {
        BranchProvider = Substitute.For<IBranchProvider>();
        ConfigureBranchProvider(BranchProvider);

        _ctx = TestDbContextFactory.Create(BranchProvider);
        Db = _ctx.Db;
    }

    /// <summary>
    /// Default: a non-global branch-scoped user. Override per-test if needed.
    /// </summary>
    protected virtual void ConfigureBranchProvider(IBranchProvider provider)
    {
        provider.GetBranchId().Returns(BranchId);
        provider.GetRequiredBranchId().Returns(BranchId);
        provider.IsGlobal().Returns(false);
    }

    public void Dispose() => _ctx.Dispose();
}
