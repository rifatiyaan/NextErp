using MediatR;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(IApplicationDbContext db)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!ShouldWrapInTransaction(typeof(TRequest)))
            return await next(cancellationToken).ConfigureAwait(false);

        if (db.Database.CurrentTransaction != null)
            return await next(cancellationToken).ConfigureAwait(false);

        await using var transaction = await db.Database
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
