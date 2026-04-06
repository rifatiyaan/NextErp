using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Common.Exceptions;

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
            ForbiddenAccessException f => (StatusCodes.Status403Forbidden, "Forbidden", f.Message),
            UnauthorizedAccessException u => (StatusCodes.Status401Unauthorized, "Unauthorized", u.Message),
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
