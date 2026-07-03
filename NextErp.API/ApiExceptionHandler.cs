using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Common.Exceptions;
using FluentValidationException = FluentValidation.ValidationException;
using ValidationException = NextErp.Application.Common.Exceptions.ValidationException;

namespace NextErp.API;

public sealed class ApiExceptionHandler(IHostEnvironment environment, ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        if (!httpContext.Request.Path.StartsWithSegments("/api"))
            return false;

        logger.LogError(exception, "Unhandled exception for {Path}", httpContext.Request.Path);

        var (status, title, detail) = MapException(exception, environment);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (exception is ValidationException vex)
        {
            problem.Extensions["errors"] = vex.Errors;
        }
        else if (exception is FluentValidationException fvex)
        {
            // Handlers occasionally throw FluentValidation's exception directly for
            // catalog-level failures discovered mid-handler (outside the
            // ValidationBehavior pipeline, e.g. CreateOnlineOrderHandler). Normalize
            // to the same field -> messages shape as the pipeline's ValidationException
            // so API consumers see one consistent error contract.
            problem.Extensions["errors"] = fvex.Errors
                .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }

        httpContext.Response.ContentType = "application/problem+json; charset=utf-8";
        httpContext.Response.StatusCode = status;

        await httpContext.Response.WriteAsJsonAsync(problem, JsonOptions, cancellationToken: cancellationToken);
        return true;
    }

    private static (int Status, string Title, string Detail) MapException(Exception exception, IHostEnvironment environment)
    {
        return exception switch
        {
            ValidationException => (
                StatusCodes.Status422UnprocessableEntity,
                "Validation failed",
                "One or more validation failures have occurred."),
            FluentValidationException => (
                StatusCodes.Status422UnprocessableEntity,
                "Validation failed",
                "One or more validation failures have occurred."),
            ForbiddenAccessException f => (StatusCodes.Status403Forbidden, "Forbidden", f.Message),
            UnauthorizedAccessException u => (StatusCodes.Status401Unauthorized, "Unauthorized", u.Message),
            InvalidOperationException op => (StatusCodes.Status400BadRequest, "Bad request", op.Message),
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Concurrency conflict",
                "The record was modified by another request. Please retry."),
            // Backstop for unique-index / constraint violations that exhaust the
            // caller's own retry (e.g. OnlineOrderNumberFactory collisions).
            // Sanitized: no inner exception/SQL details reach the client.
            DbUpdateException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "The request conflicted with existing data. Please retry."),
            ArgumentException ax => (StatusCodes.Status400BadRequest, "Bad request", ax.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                environment.IsDevelopment() ? exception.ToString() : "An error occurred processing your request."),
        };
    }
}
