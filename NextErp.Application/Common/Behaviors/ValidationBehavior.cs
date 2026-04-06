using MediatR;

namespace NextErp.Application.Common.Behaviors;

/// <summary>
/// Pipeline hook for request validation. Extend this behavior when adding FluentValidation or similar.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken) =>
        next(cancellationToken);
}
