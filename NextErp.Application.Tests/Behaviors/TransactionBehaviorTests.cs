using MediatR;
using NextErp.Application.Common.Behaviors;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Domain.Entities;
using NSubstitute;

namespace NextErp.Application.Tests.Behaviors;

public class TransactionBehaviorTests : IDisposable
{
    private readonly TestDbContextFactory.TestContext _ctx;

    public TransactionBehaviorTests()
    {
        _ctx = TestDbContextFactory.Create();
    }

    public void Dispose() => _ctx.Dispose();

    // ---- Test request types ----
    public record TransactionalCommand(string Tag) : IRequest<string>, ITransactionalRequest;
    public record NonTransactionalQuery(string Tag) : IRequest<string>;

    private TransactionBehavior<TRequest, TResponse> NewSut<TRequest, TResponse>()
        where TRequest : notnull
        => new(_ctx.Db);

    [Fact]
    public async Task NonTransactional_request_skips_transaction_and_returns_response()
    {
        var sut = NewSut<NonTransactionalQuery, string>();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next(Arg.Any<CancellationToken>()).Returns("ok");

        var result = await sut.Handle(new NonTransactionalQuery("read"), next, CancellationToken.None);

        result.Should().Be("ok");
        await next.Received(1)(Arg.Any<CancellationToken>());
        _ctx.Db.Database.CurrentTransaction.Should().BeNull("non-transactional path must not start a transaction");
    }

    [Fact]
    public async Task Transactional_request_success_commits_and_returns_response()
    {
        var sut = NewSut<TransactionalCommand, string>();

        // The "next" insert a Branch row — if commit works, row should persist.
        var branch = new BranchBuilder().WithTitle("Tx-Commit").Build();

        Task<string> Next(CancellationToken ct)
        {
            _ctx.Db.Branches.Add(branch);
            return Task.FromResult("done");
        }

        var result = await sut.Handle(new TransactionalCommand("write"), Next, CancellationToken.None);
        await _ctx.Db.SaveChangesAsync();   // simulate handler save inside the transaction scope

        result.Should().Be("done");
        _ctx.Db.Database.CurrentTransaction.Should().BeNull("transaction should be disposed after commit");
        var persisted = await _ctx.Db.Branches.AsNoTracking().AnyAsync(b => b.Id == branch.Id);
        persisted.Should().BeTrue();
    }

    [Fact]
    public async Task Transactional_request_throws_then_rollback_persists_nothing()
    {
        var sut = NewSut<TransactionalCommand, string>();
        var branch = new BranchBuilder().WithTitle("Tx-Rollback").Build();

        async Task<string> Next(CancellationToken ct)
        {
            _ctx.Db.Branches.Add(branch);
            await _ctx.Db.SaveChangesAsync(ct);   // row staged + saved inside transaction
            throw new InvalidOperationException("boom");
        }

        var act = () => sut.Handle(new TransactionalCommand("write"), Next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        _ctx.Db.Database.CurrentTransaction.Should().BeNull("transaction should be disposed after rollback");

        // Critical regression assertion: rollback must purge the row that SaveChanges flushed.
        // Use a fresh context against the same SQLite connection to bypass the change tracker
        // (the in-memory Db still has the entity tracked as Unchanged after rollback).
        var verifyOptions = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<NextErp.Infrastructure.ApplicationDbContext>()
            .UseSqlite(_ctx.Connection)
            .Options;
        using var verifyDb = new NextErp.Infrastructure.ApplicationDbContext(verifyOptions);
        var stillThere = await verifyDb.Branches.AsNoTracking().AnyAsync(b => b.Id == branch.Id);
        stillThere.Should().BeFalse("rollback must undo the insert");
    }

    [Fact]
    public async Task Already_in_transaction_does_not_double_wrap()
    {
        var sut = NewSut<TransactionalCommand, string>();
        await using var outerTx = await _ctx.Db.Database.BeginTransactionAsync();
        var outerInstance = _ctx.Db.Database.CurrentTransaction;
        outerInstance.Should().NotBeNull();

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next(Arg.Any<CancellationToken>()).Returns("ok");

        var result = await sut.Handle(new TransactionalCommand("nested"), next, CancellationToken.None);

        result.Should().Be("ok");
        // The outer transaction should still be the active one — behavior must not have started a nested one.
        _ctx.Db.Database.CurrentTransaction.Should().BeSameAs(outerInstance);
        await outerTx.RollbackAsync();
    }
}

