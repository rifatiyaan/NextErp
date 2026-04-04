using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NextErp.API;

/// <summary>
/// Maps unhandled exceptions to RFC 7807 <see cref="ProblemDetails"/> responses (JSON, camelCase).
/// </summary>
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
        CancellationToken cancellationToken)
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

        httpContext.Response.ContentType = "application/problem+json; charset=utf-8";
        httpContext.Response.StatusCode = status;

        await httpContext.Response.WriteAsJsonAsync(problem, JsonOptions, cancellationToken: cancellationToken);
        return true;
    }

    private static (int Status, string Title, string Detail) MapException(Exception exception, IHostEnvironment environment)
    {
        return exception switch
        {
            InvalidOperationException op => (StatusCodes.Status400BadRequest, "Bad request", op.Message),
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Concurrency conflict",
                "The record was modified by another request. Please retry."),
            ArgumentException ax => (StatusCodes.Status400BadRequest, "Bad request", ax.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                environment.IsDevelopment() ? exception.ToString() : "An error occurred processing your request."),
        };
    }
}
