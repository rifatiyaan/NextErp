using MediatR;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Common.Behaviors;

/// <summary>
/// Wraps write-style MediatR requests in a single database transaction (commit on success, rollback on any exception).
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IApplicationDbContext _db;

    public TransactionBehavior(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!ShouldWrapInTransaction(typeof(TRequest)))
            return await next(cancellationToken).ConfigureAwait(false);

        if (_db.Database.CurrentTransaction != null)
            return await next(cancellationToken).ConfigureAwait(false);

        await using var transaction = await _db.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var response = await next(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Treats requests whose type name suggests a command (Create / Update / Delete) as transactional; excludes queries.
    /// </summary>
    private static bool ShouldWrapInTransaction(Type requestType)
    {
        var name = requestType.Name;
        if (name.Contains("Query", StringComparison.OrdinalIgnoreCase))
            return false;

        return name.Contains("Create", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Update", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Delete", StringComparison.OrdinalIgnoreCase);
    }
}
